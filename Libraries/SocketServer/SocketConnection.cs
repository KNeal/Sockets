using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Principal;
using SocketServer.Utils;

namespace SocketServer
{
    public class SocketConnection : ISocketConnection
    {
        public static class Defaults
        {
            public const int ReadBufferLen = 1024;
        }
        
        public enum State
        {
            Disconnected,
            Connecting,
            Connected,
            Error
        }

        public delegate void ConnectionStateCallback(SocketConnection connection);
        public delegate void MessageCallback(SocketConnection connection, MemoryStream message);
        public delegate void WriteCallback(Stream outputStream);
        
        public int ConnectionId { get; set; }
        public string ConnectionName { get; set; }
        public State ConnectionState { get; private set; }
        public DateTime LastMessageTime { get; private set; }
        
        private readonly object _writeLock = new object();
        private readonly object _readLock = new object();
        private Socket _socket;

        private const int MessageHeaderSize = sizeof (UInt32);
        private readonly byte[] _readBuffer;
        private int _readMessageLength;
        private int _readMessageLengthRead;
        private readonly MemoryStream _readStream;
        private readonly MemoryStream _writeStream;

        public event ConnectionStateCallback OnConnected;
        public event ConnectionStateCallback OnDisconnected;
        public event MessageCallback OnMessage;
        
        public SocketConnection()
            : this(null, Defaults.ReadBufferLen)
        {
        }

        public SocketConnection(Socket socket)
            : this(socket, Defaults.ReadBufferLen)
        {
        }

        public SocketConnection(Socket socket, int bufferLen)
        {
            _socket = socket;
            _readBuffer = new byte[bufferLen];
            _readStream = new MemoryStream(bufferLen);
            _writeStream = new MemoryStream(bufferLen);
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
                Logger.Info("[SocketConnection] Attempting to connecting to {0}:{1}", port, host);
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

                // Add a Placeholder header
                _writeStream.SetLength(0);
                BinaryUtils.WriteUInt32(_writeStream, 0);
                
                // Write the data & calculate the length
                long lenBeforeData = _writeStream.Length;
                writeCallback(_writeStream);
                long dataLen = _writeStream.Length - lenBeforeData;
                
                // Overwite the header with the true value
                _writeStream.Seek(0, SeekOrigin.Begin);
                BinaryUtils.WriteUInt32(_writeStream, (UInt32)dataLen);
                _writeStream.Seek(0, SeekOrigin.End);

                // Send the message
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

                    Logger.Info("[SocketConnection] Successfully connected to {0}", _socket.RemoteEndPoint);

                    SetConnectionState(State.Connected);
                }
                catch (Exception e)
                {
                    Logger.Error("[SocketConnection] OnConnect Failed: {0}", e);
                }
            }
        }

        private void OnRecieve(IAsyncResult ar)
        {
            lock (_readLock)
            {
                if (_socket == null)
                {
                    return;
                }

                try
                {
                    // Update the last message time
                    LastMessageTime = DateTime.UtcNow;

                    // Read data from the client socket. 
                    int bytesRecieved = _socket.EndReceive(ar);
                    if (bytesRecieved > 0)
                    {
                        int readIndex = 0;
                        while (readIndex < bytesRecieved)
                        {
                            // Read the header
                            if (_readMessageLength == 0)
                            {
                                // The message header might be split across multiple callbacks, so always cache it to
                                // the stream first.
                                int readLen = Math.Min(MessageHeaderSize - (int)_readStream.Length, bytesRecieved - readIndex);
                                _readStream.Write(_readBuffer, readIndex, readLen);
                                readIndex += readLen;

                                if (_readStream.Length == MessageHeaderSize)
                                {
                                    // Parse the header
                                    _readStream.Seek(0, SeekOrigin.Begin);
                                    _readMessageLength = (int)BinaryUtils.ReadUInt32(_readStream);

                                    // Reset for the message data.
                                    _readStream.SetLength(0);
                                }
                            }

                            // Read the Mesage Data
                            if (_readMessageLength > 0)
                            {
                                int bytesAvailable = (int)(bytesRecieved - readIndex);
                                int readLen = Math.Min(bytesAvailable, _readMessageLength - _readMessageLengthRead);
                                _readStream.Write(_readBuffer, readIndex, readLen);
                                _readMessageLengthRead += readLen;
                                readIndex += readLen;

                                // Handle a completed message
                                if (_readMessageLength == _readMessageLengthRead)
                                {
                                    // Process the message
                                    MessageCallback callback = OnMessage;
                                    if (callback != null)
                                    {
                                        try
                                        {
                                            _readStream.Seek(0, SeekOrigin.Begin);
                                            callback(this, _readStream);
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.Error("[SocketConnection] OnMessage Handler Error: {0}", e);
                                        }
                                    }

                                    // Reset for the next message
                                    _readMessageLength = 0;
                                    _readMessageLengthRead = 0;
                                    _readStream.SetLength(0);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("[SocketConnection] OnRecieve Failed: {0}", e);
                }

                ListenForData();
            }

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