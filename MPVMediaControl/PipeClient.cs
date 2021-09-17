using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPVMediaControl
{
    class PipeClient
    {
        public static void SendCommand(string command)
        {
            new Thread(_ =>
            {
                var pipeClient = new NamedPipeClientStream("mpvsocket");
                pipeClient.Connect();

                var ss = new StreamString(pipeClient);
                ss.WriteString(command);

                pipeClient.Close();
            }).Start();
        }
    }
}
