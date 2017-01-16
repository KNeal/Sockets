using System;
using System.IO;
using System.Net.Sockets;

namespace Sockets
{
    public class SocketConnection : ISocketConnection
    {
        public enum State
        {
            Disconnected,
            Connecting,
            Authenticating,
            Connected,
            Error
        }

        public const int DefaultBufferLen = 1024;

        public int ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public State ConnectionState { get; set; }
        public DateTime LastMessageTime { get; private set; }

        private Socket _socket;
        private readonly ISocketMessageHandler _messageHandler;
        private readonly byte[] _buffer;
        private readonly MemoryStream _memoryStream;

        public delegate void ConnectionStateCallback(SocketConnection connection);

        public event ConnectionStateCallback OnConnected;
        public event ConnectionStateCallback OnDisconnected;

        public SocketConnection(ISocketMessageHandler messageHandler, int bufferLen = DefaultBufferLen)
            : this(null, messageHandler, bufferLen)
        {
        }

        public SocketConnection(Socket socket, ISocketMessageHandler messageHandler, int bufferLen = DefaultBufferLen)
        {
            _socket = socket;
            _messageHandler = messageHandler;
            _buffer = new byte[DefaultBufferLen];
            _memoryStream = new MemoryStream(DefaultBufferLen);
        }

        public void Connect(string host, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(host, port, OnConnect, this);
            Console.WriteLine("Connecting to {0}", _socket.RemoteEndPoint);
        }

        public void Disconnect()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                // Complete the connection.
                _socket.EndConnect(ar);
                ListenForData();

                Console.WriteLine("Socket connected to {0}", _socket.RemoteEndPoint);
                SetConnectionState(State.Connected);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ListenForData()
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, 0, OnRecieve, this);
        }

        public void WriteMessage(ISocketMessage message)
        {
            _messageHandler.WriteMessage(_socket, message);
        }

        private void OnRecieve(IAsyncResult ar)
        {
            // Update the last message time
            LastMessageTime = DateTime.UtcNow;

            // Read data from the client socket. 
            int bytesRead = _socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                _memoryStream.Write(_buffer, 0, bytesRead);

                _messageHandler.ReadMessage(this, _memoryStream);
            }

            // Continue Listening
            ListenForData();
        }

        private void SetConnectionState(State state)
        {
            ConnectionState = state;
            ConnectionStateCallback callback = null;

            switch (ConnectionState)
            {
                case State.Connected:
                    callback = OnConnected;
                    break;

                case State.Disconnected:
                    callback = OnDisconnected;
                    break;
            }

            if (callback != null)
            {
                callback(this);
            }
        }
    }
}