using System;
using System.IO;
using System.Net.Sockets;

namespace Sockets
{
    public class SocketConnection
    {
        public const int DefaultBufferLen = 1024;

        public Socket Socket { get; private set; }
        public ISocketMessageHandler MessageHandler { get; private set; }
        public byte[] Buffer { get; private set; }
        public MemoryStream MemoryStream { get; private set; }
        public DateTime LastMessageTime { get; private set; }

        public SocketConnection(Socket socket, ISocketMessageHandler messageHandler, int bufferLen = DefaultBufferLen)
        {
            Socket = socket;
            MessageHandler = messageHandler;
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

        public void ListenForData()
        {
            Socket.BeginReceive(Buffer, 0, Buffer.Length, 0, OnRecieve, this);
        }

        private void OnRecieve(IAsyncResult ar)
        {
            // Update the last message time
            LastMessageTime = DateTime.UtcNow;

            // Read data from the client socket. 
            int bytesRead = Socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                MemoryStream.Write(Buffer, 0, bytesRead);

                MessageHandler.ReadMessage(this, MemoryStream);
            }

            // Continue Listening
            ListenForData();
        }
    }
}