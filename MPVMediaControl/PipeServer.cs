using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using Windows.Media;

namespace MPVMediaControl
{
    class PipeServer
    {
        private const int NumThreads = 16;
        private static readonly Thread[] Servers = new Thread[NumThreads];

        private static readonly object ParseLock = new object();
        private static ConcurrentQueue<int> CommandQueue = new ConcurrentQueue<int>();

        public static void StartServer()
        {
            for (var i = 0; i < NumThreads; i++)
            {
                Servers[i] = new Thread(ServerThread);
                Servers[i].Start();
            }
        }

        public static void Cleanup()
        {
            for (var i = 0; i < NumThreads; i++)
            {
                if (Servers[i] != null)
                    Servers[i].Abort();
            }
        }

        private static void ServerThread(object data)
        {
            while (true)
            {
                using (NamedPipeServerStream pipeServer =
                    new NamedPipeServerStream("mpvmcsocket", PipeDirection.InOut, NumThreads))
                {
                    try
                    {
                        pipeServer.WaitForConnection();
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Received message");
#endif
                        CommandQueue.Enqueue(Thread.CurrentThread.ManagedThreadId);

                        var ss = new StreamString(pipeServer);
                        var command = ss.ReadString();

#if DEBUG
                        System.Diagnostics.Debug.WriteLine(command);
#endif
                        while (true)
                        {
                            lock (ParseLock)
                            {
#if DEBUG
                                CommandQueue.TryPeek(out var nextId);
                                System.Diagnostics.Debug.WriteLine(
                                    $"Next ID is {nextId}, current thread is {Thread.CurrentThread.ManagedThreadId}");
#endif
                                if (!CommandQueue.TryPeek(out var id) || id != Thread.CurrentThread.ManagedThreadId)
                                    continue;
                                CommandQueue.TryDequeue(out _);
                                ParseCommand(command);
                                break;
                            }
                        }
                    }

                    catch (IOException)
                    {
                    }
                    catch (ThreadAbortException)
                    {
                        pipeServer.Close();
                        break;
                    }
                }
            }
        }

        private static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.UTF8.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }

        private static void ParseFile(MediaController controller, Dictionary<string, string> parameters)
        {
            var title = FromHexString(parameters["title"]);
            var artist = FromHexString(parameters["artist"]);
            var path = FromHexString(parameters["path"]);
            var shotPath = parameters.ContainsKey("shot_path") ? FromHexString(parameters["shot_path"]) : String.Empty;

            // Using MediaPlaybackType.Unknown will cause exception, so another default value has to be set
            var type = MediaPlaybackType.Music;
            if (parameters.ContainsKey("type"))
            {
                switch (parameters["type"])
                {
                    case "video":
                        type = MediaPlaybackType.Video;
                        break;
                    case "music":
                        type = MediaPlaybackType.Music;
                        break;
                    case "image":
                        type = MediaPlaybackType.Image;
                        break;
                }
            }

            // Processing metadata may take some time, so only checking path isn't enough.
            if (title == controller.File.Title &&
                artist == controller.File.Artist &&
                path == controller.File.Path)
                return;

            var file = new MediaController.MCMediaFile
            {
                Title = title,
                Artist = artist,
                Path = path,
                ShotPath = shotPath.Replace('/', '\\'),
                Type = type,
            };

            controller.File = file;
        }

        private static void ParseCommand(string command)
        {
            if (command.EndsWith(" \r\n"))
                command = command.Substring(0, command.Length - 3);
            if (command.StartsWith("^") && command.EndsWith("$"))
            {
                command = command.TrimStart('^').TrimEnd('$');
                var commandName = command.Split('[')[1].Split(']')[0];

                var parameters = new Dictionary<string, string>();
                var parameterStrs = command.Split('(');

                foreach (var parameterStr in parameterStrs)
                {
                    if (parameterStr.Last() != ')')
                        continue;
                    var parameter = parameterStr.Substring(0, parameterStr.Length - 1);
                    var parameterName = parameter.Split('=')[0];
                    var parameterValue = parameter.Split('=')[1];

                    parameters.Add(parameterName, parameterValue);
                }

#if DEBUG
                System.Diagnostics.Debug.WriteLine(commandName);
#endif

                var pid = int.Parse(parameters["pid"]);
                var socketName = parameters["socket_name"];
                var controller = Program.AppContext.GetController(pid, socketName);

                switch (commandName)
                {
                    case "setFile":
                        ParseFile(controller, parameters);
                        break;

                    case "setState":
                        var isPlaying = parameters["playing"] == "true";
                        if (Program.AppContext != null && controller != null && controller.File.Path != null)
                        {
                            var expectedState =
                                isPlaying ? MediaController.PlayState.Play : MediaController.PlayState.Pause;
                            if (controller.State != expectedState)
                                controller.State = expectedState;
                        }

                        break;

                    case "setQuit":
                        var quit = parameters["quit"] == "true";
                        if (quit)
                        {
                            if ((Program.AppContext != null && controller != null) && controller.File.Path != null)
                            {
                                if (controller.State != MediaController.PlayState.Stop)
                                    controller.State = MediaController.PlayState.Stop;
                                Program.AppContext.RemoveController(pid);

                                Program.AppContext.ExitIfNoControllers();
                            }
                        }

                        break;

                    case "setShot":
                        var shotPath = parameters["shot_path"];
                        controller.UpdateShotPath(FromHexString(shotPath));
                        break;
                }
            }
        }
    }
}