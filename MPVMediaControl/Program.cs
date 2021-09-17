using System;
using System.Drawing;
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

        public MainForm Main;
        public MediaController Controller;

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

            Main = new MainForm();
            Main.Show();

            Controller = new MediaController(Main.Handle);
        }

        private void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;

            PipeServer.Cleanup();

            Controller?.Cleanup();

            Application.Exit();
            Environment.Exit(0);
        }
    }
}
