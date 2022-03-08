using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly int _formIndex;

        public readonly int Pid;

        public class MCMediaFile
        {
            public string Title;
            public string Artist;
            public string Path;
            public string ShotPath;

            private static readonly string[] AudioFormats =
            {
                "m4a", "wma", "aac", "adt", "adts", "mp3", "wav", "ac3", "ec3", "flac", "ape", "tta", "tak", "ogg",
                "opus",
            };

            private static readonly string[] VideoFormats =
            {
                "3g2", "3gp2", "3gp", "3gpp", "m4v", "mp4v", "mp4", "mov", "m2ts", "asf", "wmv", "avi", "mkv",
            };

            private static readonly string[] ImageFormats =
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

            public bool ThumbnailObtained = false;

            private IStorageFile _thumbnailFile;

            public IStorageFile ThumbnailFile()
            {
                if (!ThumbnailObtained)
                {
                    Thread.Sleep(100);
                    var count = 0;
                    while (!System.IO.File.Exists(ShotPath) && count++ < 50)
                    {
                        Thread.Sleep(100);
                    }

                    ThumbnailObtained = true;
                    _thumbnailFile = StorageFile.GetFileFromPathAsync(ShotPath).GetAwaiter().GetResult();
                }

                return _thumbnailFile;
            }

            public void Cleanup()
            {
                if (System.IO.File.Exists(ShotPath))
                {
                    System.IO.File.Delete(ShotPath);
                }
            }
        }

        private readonly MCMediaFile _file = new MCMediaFile();

        public MCMediaFile File
        {
            get => _file;
            set
            {
                _file.Title = value.Title;
                _file.Artist = value.Artist;
                if (_file.Path != value.Path)
                    _file.ThumbnailObtained = false;
                _file.Path = value.Path;
                _file.ShotPath = value.ShotPath.Replace('/', '\\');

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

                try
                {
                    _updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(_file.ThumbnailFile());
                }
                catch (Exception e) when (e is FileNotFoundException || e is UnauthorizedAccessException ||
                                          e is ArgumentException)
                {
                }

                _updater.Update();
            }
        }

        public enum PlayState
        {
            Play,
            Pause,
            Stop
        }

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

        public MediaController(int pid)
        {
            this.Pid = pid;
            var (index, hWnd) = Program.AppContext.CreateForm();
            _formIndex = index;

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
            _updater.ClearAll();
            Program.AppContext.RemoveForm(_formIndex);
        }

        private void ButtonPressed(SystemMediaTransportControls controls,
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

        private void Play()
        {
            PipeClient.SendCommand(Pid, "{ \"command\": [\"set_property\", \"pause\", false] }\r\n");
        }

        private void Pause()
        {
            PipeClient.SendCommand(Pid, "{ \"command\": [\"set_property\", \"pause\", true] }\r\n");
        }

        private void Next()
        {
            PipeClient.SendCommand(Pid, "{ \"command\": [\"playlist-next\", \"weak\"] }\r\n");
        }

        private void Previous()
        {
            PipeClient.SendCommand(Pid, "{ \"command\": [\"playlist-prev\", \"weak\"] }\r\n");
        }
    }
}