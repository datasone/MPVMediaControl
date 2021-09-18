using System;
using System.IO;
using System.Text;

namespace MPVMediaControl
{
    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString
    {
        private Stream ioStream;
        private Encoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UTF8Encoding();
        }

        public string ReadString()
        {
            int len = 1024;

            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer).TrimEnd('\0');
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }

            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length;
        }
    }
}