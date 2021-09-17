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
        
        public static List<MediaController> Controllers;

        public MyAppContext()
        {
            _trayIcon = new NotifyIcon()
            {
                Text = "MPV Media Control",
                Icon = new Icon(SystemIcons.Application, 32, 32),
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            PipeServer.StartServer();

            Controllers = new List<MediaController>();
        }
        
        public MediaController GetController(int pid)
        {
            if (Controllers.FindIndex(c => c.pid == pid) == -1)
            {
                Controllers.Add(new MediaController(pid));
                return Controllers.Last();
            }
            return Controllers.Find(c => c.pid == pid);
        }

        private void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;

            PipeServer.Cleanup();

            Controllers.ForEach(i => i.Cleanup());

            Application.Exit();
            Environment.Exit(0);
        }
    }
}
