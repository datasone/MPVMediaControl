using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPVMediaControl
{
    internal static class Program
    {
        public static MyAppContext AppContext;
        private const string Guid = "851aefa6-d429-4f3b-9047-7e08d35810ad";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using (Mutex mutex = new Mutex(false, "Global\\CrystalLyrics_" + Guid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    System.Diagnostics.Debug.WriteLine("Another instance of this application is already running.");
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                AppContext = new MyAppContext();
                Application.Run(AppContext);
            }
        }
    }

    public class MyAppContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private static List<MediaController> _controllers;

        public MyAppContext()
        {
            _trayIcon = new NotifyIcon
            {
                Text = "MPV Media Control",
                Icon = new Icon(SystemIcons.Application, 32, 32),
                ContextMenu = new ContextMenu(new []
                {
                    new MenuItem("Edit media control commands", ShowEditWindow),
                    new MenuItem("Reset SMTC", ResetControllers),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            PipeServer.StartServer();

            _controllers = new List<MediaController>();
        }

        public MediaController GetController(int pid, string socketName)
        {
            if (_controllers.FindIndex(c => c.Pid == pid) == -1)
            {
                _controllers.Add(new MediaController(pid, socketName, true));
                return _controllers.Last();
            }

            return _controllers.Find(c => c.Pid == pid);
        }

        public void RemoveController(int pid)
        {
            var controller = _controllers.Find(c => c.Pid == pid);
            controller.Cleanup(true);
            _controllers.Remove(controller);
        }

        private static void ShowEditWindow(object sender, EventArgs e)
        {
            new CommandEditor().Show();
        }

        private async void ResetControllers(object sender, EventArgs e)
        {
            var newControllers = _controllers.Select(c => c.DuplicateSelf()).ToList();

            _controllers.ForEach(c => c.Cleanup(false));
            _controllers.Clear();
            
            // We need to manually trigger GC to remove MusicPlayer instances and prevent duplicate SMTC controls
            GC.Collect();
            
            _controllers.AddRange(newControllers);
            
            // There is a bug in Windows where the control is not visible (but exists and is able to interact) if the info is updated too fast
            // Sleep for a bit to prevent this
            foreach (var controller in _controllers)
            {
                await Task.Delay(400);
                controller.InitSMTC();
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;

            PipeServer.Cleanup();

            _controllers.ForEach(i => i.Cleanup(true));

            Application.Exit();
            Environment.Exit(0);
        }

        public async void ExitIfNoControllers()
        {
            if (_controllers.Count == 0)
            {
                await Task.Run(() => Exit(null, null));
            }
        }
    }
}