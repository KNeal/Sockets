using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
    public class SocketConnection : ISocketConnection
    {
        public static class Defaults
        {
            public const int ReadBufferLen = 1024;
            public static readonly byte[] EndOfMessageBytes = Encoding.ASCII.GetBytes("[EOM]");
        }
        
        public enum State
        {
            Disconnected,
            Connecting,
            Connected,
            Error
        }

        public delegate void ConnectionStateCallback(SocketConnection connection);
        public delegate void MessageCallback(SocketConnection connection, MemoryStream stream);
        public delegate void WriteCallback(Stream outputStream);
        
        public int ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public State ConnectionState { get; private set; }
        public DateTime LastMessageTime { get; private set; }
        
        private readonly object _writeLock = new object();
        private readonly object _readLock = new object();
        private Socket _socket;
        private readonly byte[] _readBuffer;
        private readonly MemoryStream _readStream;
        private readonly MemoryStream _writeStream;
        private readonly byte[] _endOfMessageBytes;

        public event ConnectionStateCallback OnConnected;
        public event ConnectionStateCallback OnDisconnected;
        public event MessageCallback OnMessage;
        
        public SocketConnection()
            : this(null, Defaults.ReadBufferLen, Defaults.EndOfMessageBytes)
        {
        }

        public SocketConnection(Socket socket)
            : this(socket, Defaults.ReadBufferLen, Defaults.EndOfMessageBytes)
        {
        }

        public SocketConnection(Socket socket, int bufferLen, byte[] eomBytes)
        {
            _socket = socket;
            _readBuffer = new byte[bufferLen];
            _readStream = new MemoryStream(bufferLen);
            _writeStream = new MemoryStream(bufferLen);
            _endOfMessageBytes = eomBytes;

            // TODO: find a more robust way for intializing with and without an open socket.
            SetConnectionState(_socket != null? State.Connected : State.Disconnected);
        }

        public void Connect(string host, int port)
        {
            lock (_writeLock)
            lock (_readLock)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.BeginConnect(host, port, OnConnect, this);
                Console.WriteLine("[SocketConnection] Attempting to connecting to {0}:{1}", port, host);
            }
        }

        public void Disconnect()
        {
            lock (_writeLock)
            lock (_readLock)
            {
                if (_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _socket = null;
                }
            }
        }

        public void WriteMessage(WriteCallback writeCallback)
        {
            lock (_writeLock)
            {
                if (ConnectionState != State.Connected)
                {
                    throw new Exception("Socket is not connected");
                }

                // Write the data
                _writeStream.SetLength(0);
                writeCallback(_writeStream);

                // Write the EOM
                _writeStream.Write(_endOfMessageBytes, 0, _endOfMessageBytes.Length);
                _socket.Send(_writeStream.ToArray());
            }
        }

        public void ListenForData()
        {
            lock (_readLock)
            {
                if (_socket != null)
                {
                    _socket.BeginReceive(_readBuffer, 0, _readBuffer.Length, 0, OnRecieve, this);
                }
            }
        }

        #region Private Methods
        private void OnConnect(IAsyncResult ar)
        {
            lock (_readLock)
            {
                try
                {
                    // Complete the connection.
                    _socket.EndConnect(ar);
                    ListenForData();

                    Console.WriteLine("[SocketConnection] Successfully connected to {0}", _socket.RemoteEndPoint);

                    SetConnectionState(State.Connected);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[SocketConnection] OnConnect Failed: {0}", e);
                }
            }
        }

        private void OnRecieve(IAsyncResult ar)
        {
            lock (_readLock)
            {
                try
                {
                    // Update the last message time
                    LastMessageTime = DateTime.UtcNow;

                    // Read data from the client socket. 
                    int bytesRead = _socket.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        _readStream.Write(_readBuffer, 0, bytesRead);
                    }

                    // Handle a completed message
                    if (IsEndOfMessage(_readBuffer, bytesRead))
                    {
                        ReadMessage();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[SocketConnection] OnRecieve Failed: {0}", e);
                }

                ListenForData();
            }

        }

        private bool IsEndOfMessage(byte[] buffer, int len)
        {
            if (buffer == null || buffer.Length < _endOfMessageBytes.Length || len < _endOfMessageBytes.Length)
            {
                return false;
            }

            // Compare the last N bytes in the buffe against the EOM values.
            int startPos = len - _endOfMessageBytes.Length;
            for (int i = 0; i < _endOfMessageBytes.Length; ++i)
            {
                if (buffer[startPos + i] != _endOfMessageBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void ReadMessage()
        {
            // Strip off the EOM Bytes
            _readStream.SetLength(_readStream.Length - _endOfMessageBytes.Length);

            // Reset the stream position to the begining.
            _readStream.Seek(0, 0);

            MessageCallback callback = OnMessage;
            if (callback != null)
            {
                try
                {
                    callback(this, _readStream);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[SocketConnection] OnMessage Handler Error: {0}", e);
                }
            }

            // Reset the buffer
            _readStream.SetLength(0);
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
        #endregion
    }
}