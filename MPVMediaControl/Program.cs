using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MPVMediaControl
{
    internal static class Program
    {
        public static MyAppContext AppContext;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppContext = new MyAppContext();
            Application.Run(AppContext);
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
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            PipeServer.StartServer();

            _controllers = new List<MediaController>();
        }

        public MediaController GetController(int pid)
        {
            if (_controllers.FindIndex(c => c.Pid == pid) == -1)
            {
                _controllers.Add(new MediaController(pid));
                return _controllers.Last();
            }

            return _controllers.Find(c => c.Pid == pid);
        }

        public void RemoveController(int pid)
        {
            var controller = _controllers.Find(c => c.Pid == pid);
            controller.Cleanup();
            _controllers.Remove(controller);
        }

        private void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;

            PipeServer.Cleanup();

            _controllers.ForEach(i => i.Cleanup());

            Application.Exit();
            Environment.Exit(0);
        }
    }
}