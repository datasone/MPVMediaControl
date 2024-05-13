using System;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;

namespace MPVMediaControl
{
    public class MediaController
    {
        private SystemMediaTransportControls _controls;
        private SystemMediaTransportControlsDisplayUpdater _updater;
        private MediaPlayer _mediaPlayer;

        public readonly int Pid;
        private readonly string SocketName;

        public class MCMediaFile
        {
            public string Title;
            public string Artist;
            public string Path;
            public string ShotPath;
            public MediaPlaybackType Type;

            public bool ThumbnailObtained = false;

            private IStorageFile _thumbnailFile;

            public IStorageFile ThumbnailFile()
            {
                if (!ThumbnailObtained && ShotPath != "")
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

            public MCMediaFile Clone()
            {
                return this.MemberwiseClone() as MCMediaFile;
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
                    _file.ThumbnailObtained = false;
                _file.Path = value.Path;
                _file.ShotPath = value.ShotPath.Replace('/', '\\');
                _file.Type = value.Type;

                _updater.ClearAll();

                _updater.Type = _file.Type;

                switch (_file.Type)
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
                    var file = _file.ThumbnailFile();
                    if (file != null)
                    {
                        _updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(file);
                    }
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

        public void InitSMTC()
        {
            _mediaPlayer = new MediaPlayer();
            _controls = _mediaPlayer.SystemMediaTransportControls;
            _mediaPlayer.CommandManager.IsEnabled = false;

            _updater = _controls.DisplayUpdater;

            _controls.ButtonPressed += ButtonPressed;
            _controls.IsPlayEnabled = true;
            _controls.IsPauseEnabled = true;
            _controls.IsNextEnabled = true;
            _controls.IsPreviousEnabled = true;

            State = _state;

            if (_file.Path != null)
            {
                File = _file;
            }
        }
        
        public MediaController(int pid, string socketName, bool initSMTC)
        {
            Pid = pid;
            SocketName = socketName;
            _state = PlayState.Stop;

            if (initSMTC)
            {
                InitSMTC();
            }
        }

        public MediaController DuplicateSelf()
        {
            var newObj = new MediaController(Pid, SocketName, false);
            newObj._file = _file.Clone();
            newObj._state = _state;
            return newObj;
        }

        public void Cleanup(bool cleanFile)
        {
            if (cleanFile)
            {
                _file?.Cleanup();
            }
            _updater.ClearAll();
            
            // Ensure objects collected while "resetting SMTC"
            _updater = null;
            _controls = null;
            _mediaPlayer = null;
        }

        public void UpdateShotPath(string path)
        {
            var shotPath = path.Replace('/', '\\');
            shotPath = shotPath.Replace("\\\\", "\\");
            File.ShotPath = shotPath;
            File.ThumbnailObtained = false;

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
            PipeClient.SendCommand(SocketName, CommandToMpv(Properties.Settings.Default.PlayCommand));
        }

        private void Pause()
        {
            PipeClient.SendCommand(SocketName, CommandToMpv(Properties.Settings.Default.PauseCommand));
        }

        private void Next()
        {
            PipeClient.SendCommand(SocketName, CommandToMpv(Properties.Settings.Default.NextCommand));
        }

        private void Previous()
        {
            PipeClient.SendCommand(SocketName, CommandToMpv(Properties.Settings.Default.PrevCommand));
        }

        private string CommandToMpv(string cmd)
        {
            var args = cmd.Split(' ');
            var argsParsed = args.Select(s =>
            {
                if (s == "true" || s == "false") return s;
                if (long.TryParse(s, out _) || float.TryParse(s, out _)) return s;
                return $"\"{s}\"";
            });

            var cmdArgs = string.Join(", ", argsParsed);
            return $"{{ \"command\": [{cmdArgs}] }}\r\n";
        }
    }
}