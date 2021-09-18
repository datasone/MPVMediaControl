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
        private PlaceholderForm _form;
        private IntPtr _handle;
        
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

            _form = new PlaceholderForm();
            _form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            _form.ShowInTaskbar = false;
            _form.StartPosition = FormStartPosition.Manual;
            _form.Location = new System.Drawing.Point(int.MinValue, int.MinValue);
            _form.Size = new System.Drawing.Size(1, 1);
            _form.Show();
            _handle = _form.Handle;
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

        public void RemoveController(int pid)
        {
            Controllers.RemoveAll(c => c.pid == pid);
        }

        public IntPtr GethWnd()
        {
            return _handle;
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
