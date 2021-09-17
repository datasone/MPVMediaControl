using System;
using System.Diagnostics;
using System.Linq;
using UWPInterop;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MPVMediaControl
{
    public class MediaController
    {
        private readonly SystemMediaTransportControls _controls;
        private readonly SystemMediaTransportControlsDisplayUpdater _updater;

        public class MCMediaFile
        {
            public string Title;
            public string Artist;
            public string Path;

            private static readonly string[] AudioFormats = new string[]
            {
                "m4a", "wma", "aac", "adt", "adts", "mp3", "wav", "ac3", "ec3", "flac", "ape", "tta", "tak", "ogg", "opus",
            };

            private static readonly string[] VideoFormats = new string[]
            {
                "3g2", "3gp2", "3gp", "3gpp", "m4v", "mp4v", "mp4", "mov", "m2ts", "asf", "wmv", "avi", "mkv",
            };

            private static readonly string[] ImageFormats = new string[]
            {
                "tif", "tiff", "png", "jpg", "gif"
            };

            public MediaPlaybackType Type()
            {
                var fileName = Path.Split('\\').Last();
                if (fileName.Split('.').Length == 1)
                    return MediaPlaybackType.Unknown;

                var extName = fileName.Split('.').Last();
                if (AudioFormats.Contains(extName))
                    return MediaPlaybackType.Music;
                if (VideoFormats.Contains(extName))
                    return MediaPlaybackType.Video;
                if (ImageFormats.Contains(extName))
                    return MediaPlaybackType.Image;

                return MediaPlaybackType.Unknown;
            }

            public bool ThumbnailGenerated = false;

            private readonly string _tmpFileName = System.IO.Path.GetTempFileName();

            private IStorageFile _thumbnailFile;

            public IStorageFile ThumbnailFile()
            {
                if (!ThumbnailGenerated)
                {
                    switch (Type())
                    {
                        case MediaPlaybackType.Image:
                            var imageArgs = new[]
                            {
                                "-y", "-nostats", "-loglevel", "0", "-i", Path, "-vf", "scale=400:-1",
                                "-f", "mjpeg", _tmpFileName,
                            };
                            var imageProcess = new Process();
                            imageProcess.StartInfo.FileName = "ffmpeg.exe";
                            imageProcess.StartInfo.Arguments = Utils.EscapeArguments(imageArgs);
                            imageProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            imageProcess.StartInfo.CreateNoWindow = true;
                            imageProcess.Start();
                            imageProcess.WaitForExit();
                            break;
                        case MediaPlaybackType.Music:
                            var musicArgs = new[]
                            {
                                "-y", "-nostats", "-loglevel", "0", "-i", Path, "-an", "-vf", "scale=400:-1",
                                "-f", "mjpeg", _tmpFileName,
                            };
                            var musicProcess = new Process();
                            musicProcess.StartInfo.FileName = "ffmpeg.exe";
                            musicProcess.StartInfo.Arguments = Utils.EscapeArguments(musicArgs);
                            musicProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            musicProcess.StartInfo.CreateNoWindow = true;
                            musicProcess.Start();
                            musicProcess.WaitForExit();
                            break;
                        case MediaPlaybackType.Video:
                            var startSeconds = -1;

                            var videoProbeArgs = new[]
                            {
                                "-loglevel", "0", "-show_entries", "stream=duration", Path
                            };
                            var videoProbeProcess = new Process();
                            videoProbeProcess.StartInfo.RedirectStandardOutput = true;
                            videoProbeProcess.StartInfo.FileName = "ffprobe.exe";
                            videoProbeProcess.StartInfo.Arguments = Utils.EscapeArguments(videoProbeArgs);
                            videoProbeProcess.StartInfo.UseShellExecute = false;
                            videoProbeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            videoProbeProcess.StartInfo.CreateNoWindow = true;
                            videoProbeProcess.Start();
                            var output = videoProbeProcess.StandardOutput.ReadToEnd();
                            videoProbeProcess.WaitForExit();
                            foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                            {
                                if (line.StartsWith("duration="))
                                {
                                    var durationStr = line.Substring(9, line.Length - 9).Split('.')[0];
                                    if (int.TryParse(durationStr, out var durationValue))
                                    {
                                        startSeconds = durationValue / 4;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }

                            if (startSeconds == -1)
                                startSeconds = 30;

                            var videoArgs = new[]
                            {
                                "-y", "-nostats", "-loglevel", "0", "-i", Path, "-vframes", "1", "-an",
                                "-vf", "scale=400:-1", "-ss", startSeconds.ToString(), "-f", "mjpeg", _tmpFileName,
                            };
                            var videoProcess = new Process();
                            videoProcess.StartInfo.FileName = "ffmpeg.exe";
                            videoProcess.StartInfo.Arguments = Utils.EscapeArguments(videoArgs);
                            videoProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            videoProcess.StartInfo.CreateNoWindow = true;
                            videoProcess.Start();
                            videoProcess.WaitForExit();
                            break;
                    }

                    ThumbnailGenerated = true;
                    _thumbnailFile = StorageFile.GetFileFromPathAsync(_tmpFileName).GetAwaiter().GetResult();
                }

                return _thumbnailFile;
            }

            public void Cleanup()
            {
                if (System.IO.File.Exists(_tmpFileName))
                {
                    System.IO.File.Delete(_tmpFileName);
                }
            }
        }

        private MCMediaFile _file = new MCMediaFile();

        public MCMediaFile File
        {
            get => _file;
            set
            {
                _file.Title = value.Title;
                _file.Artist = value.Artist;
                if (_file.Path != value.Path)
                    _file.ThumbnailGenerated = false;
                _file.Path = value.Path;

                _updater.ClearAll();

                _updater.Type = _file.Type();

                switch (_file.Type())
                {
                    case MediaPlaybackType.Image:
                        _updater.ImageProperties.Title = _file.Title;
                        break;
                    case MediaPlaybackType.Music:
                        _updater.MusicProperties.Title = _file.Title;
                        _updater.MusicProperties.Artist = _file.Artist;
                        break;
                    case MediaPlaybackType.Video:
                        _updater.VideoProperties.Title = _file.Title;
                        break;
                }

                _updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(_file.ThumbnailFile());

                _updater.Update();
            }
        }

        public enum PlayState { Play, Pause, Stop }

        private PlayState _state;
        public PlayState State
        {
            get => _state;
            set
            {
                _state = value;
                switch (value)
                {
                    case PlayState.Play:
                        _controls.IsEnabled = true;
                        _controls.PlaybackStatus = MediaPlaybackStatus.Playing;
                        break;
                    case PlayState.Pause:
                        _controls.IsEnabled = true;
                        _controls.PlaybackStatus = MediaPlaybackStatus.Paused;
                        break;
                    case PlayState.Stop:
                        _updater.ClearAll();
                        _controls.IsEnabled = false;
                        _controls.PlaybackStatus = MediaPlaybackStatus.Closed;
                        break;
                }
            }
        }

        public MediaController(IntPtr hWnd)
        {
            _controls = SystemMediaTransportControlsInterop.GetForWindow(hWnd);
            _updater = _controls.DisplayUpdater;

            _controls.ButtonPressed += ButtonPressed;
            _controls.IsPlayEnabled = true;
            _controls.IsPauseEnabled = true;
            _controls.IsNextEnabled = true;
            _controls.IsPreviousEnabled = true;

            State = PlayState.Stop;
        }

        public void Cleanup()
        {
            _file?.Cleanup();
        }

        private static void ButtonPressed(SystemMediaTransportControls controls,
            SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Pause();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Next();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Previous();
                    break;
            }
        }

        private static void Play()
        {
            PipeClient.SendCommand("{ \"command\": [\"set_property\", \"pause\", false] }\r\n");
        }

        private static void Pause()
        {
            PipeClient.SendCommand("{ \"command\": [\"set_property\", \"pause\", true] }\r\n");
        }
        private static void Next()
        {
            PipeClient.SendCommand("{ \"command\": [\"playlist-next\", \"weak\"] }\r\n");
        }
        private static void Previous()
        {
            PipeClient.SendCommand("{ \"command\": [\"playlist-prev\", \"weak\"] }\r\n");
        }
    }
}
