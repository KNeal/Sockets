using System;
using System.IO;
using System.Net.Sockets;

namespace Sockets
{
    public class SocketState
    {
        public const int DefaultBufferLen = 1024;

        public Socket Socket { get; private set; }
        public byte[] Buffer { get; private set; }
        public MemoryStream MemoryStream { get; private set; }
        public DateTime LastMessageTime { get; set; }

        public SocketState(Socket socket, int bufferLen = DefaultBufferLen)
        {
            Socket = socket;
            Buffer = new byte[DefaultBufferLen];
            MemoryStream = new MemoryStream(DefaultBufferLen);
        }

        public void Close()
        {
            if (Socket != null)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                Socket = null;
            }
        }
    }
}