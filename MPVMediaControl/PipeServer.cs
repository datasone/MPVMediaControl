using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace MPVMediaControl
{
    class PipeServer
    {
        private const int NumThreads = 10;
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
                        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Received message");
#endif
                        CommandQueue.Enqueue(Thread.CurrentThread.ManagedThreadId);

                        var ss = new StreamString(pipeServer);
                        var command = ss.ReadString();

#if DEBUG
                        // Console.WriteLine(command);
#endif
                        while (true)
                        {
                            lock (ParseLock)
                            {
#if DEBUG
                                CommandQueue.TryPeek(out var nextId);
                                Console.WriteLine(
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

        private static string ParseEDL(string edlFilePath)
        {
            // Only select first file as syncing play time to decide file is too much for thumbnail generation.
            var content = File.ReadAllLines(edlFilePath);
            foreach (var line in content)
            {
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("%"))
                {
                    var byteLenStr = line.Split('%')[1];
                    var success = int.TryParse(byteLenStr, out var byteLength);
                    if (!success)
                        continue;
                    var restPart = line.Substring(2 + byteLenStr.Length);
                    var restPartInBytes = Encoding.UTF8.GetBytes(restPart);
                    var fileNameInBytes = restPartInBytes.Take(byteLength).ToArray();
                    return Encoding.UTF8.GetString(fileNameInBytes);
                }
                else
                {
                    return line.Split(',')[0];
                }
            }

            return "";
        }

        private static string ParseCUE(string cueFilePath)
        {
            var parentPath = cueFilePath.Substring(0, cueFilePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            var content = File.ReadAllLines(cueFilePath);
            foreach (var line in content)
            {
                if (!line.StartsWith("FILE")) continue;
                var prefixLen = 6; // "FILE \""
                var suffixLen = line.Split('"').Last().Length + 1;
                return parentPath + line.Substring(prefixLen, line.Length - prefixLen - suffixLen);
            }

            return "";
        }

        private static void ParseFile(Dictionary<string, string> parameters)
        {
            var title = parameters["title"];
            var artist = parameters["artist"];
            var path = parameters["path"];

            // Processing metadata may take some time, so only checking path isn't enough.
            if (title == Program.AppContext.Controller.File.Title &&
                artist == Program.AppContext.Controller.File.Artist &&
                path == Program.AppContext.Controller.File.Path)
                return;

            if (path.Split('.').Last() == "edl")
            {
                parameters["path"] = ParseEDL(path);
                ParseFile(parameters);
            }
            else if (path.Split('.').Last() == "cue")
            {
                parameters["path"] = ParseCUE(path);
                ParseFile(parameters);
            }
            else
            {
                var file = new MediaController.MCMediaFile
                {
                    Title = title,
                    Artist = artist,
                    Path = path
                };

                Program.AppContext.Controller.File = file;
            }
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
                    parameterValue = parameterValue.Replace("\\\\[", "(").Replace("\\\\]", ")");

                    parameters.Add(parameterName, parameterValue);
                }

#if DEBUG
                Console.WriteLine(commandName);
#endif

                switch (commandName)
                {
                    case "setFile":
                        ParseFile(parameters);
                        break;

                    case "setState":
                        var isPlaying = parameters["playing"] == "true";
                        if (Program.AppContext != null && Program.AppContext.Controller != null && Program.AppContext.Controller.File.Path != null)
                        {
                            var expectedState =
                                isPlaying ? MediaController.PlayState.Play : MediaController.PlayState.Pause;
                            if (Program.AppContext.Controller.State != expectedState)
                                Program.AppContext.Controller.State = expectedState;
                        }
                        break;

                    case "setQuit":
                        var quit = parameters["quit"] == "true";
                        if (quit)
                        {
                            if ((Program.AppContext != null && Program.AppContext.Controller != null) && Program.AppContext.Controller.File.Path != null)
                            {
                                if (Program.AppContext.Controller.State != MediaController.PlayState.Stop)
                                    Program.AppContext.Controller.State = MediaController.PlayState.Stop;
                            }
                        }
                        break;
                }
            }
        }
    }

}