using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Sockets
{
    public abstract class SocketClient : SocketMessageHandler
    {
        public enum State
        {
            Disconnected,
            Connecting,
            Connected
        }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly string _host;
        private readonly int _port;
        private SocketConnection _connection;
        private Timer _updateTimer;
        
        public State ConnectionState { get; private set; }

        protected SocketClient(string host, int port)
        {
            _host = host;
            _port = port;
            ConnectionState = State.Disconnected;
        }
        
        public void Start()
        {
            ConnectionState = State.Connecting;
            _updateTimer =  new Timer(OnUpdate, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public void Stop()
        {
            _lock.EnterWriteLock();
            try
            {
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Close();
                    _connection = null;
                }

                if (_updateTimer != null)
                {
                    _updateTimer.Dispose();
                    _updateTimer = null;
                }

                ConnectionState = State.Disconnected;
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        public void SendMessage(ISocketMessage message)
        {
            WriteMessage(_connection.Socket, message);
        }

        protected abstract void OnMessage(ISocketMessage message);
        
        #region Private Methods

        private void CreateConnection()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _connection = new SocketConnection(socket);
            _connection.Socket.BeginConnect(_host, _port, OnConnect, _connection);
            Console.WriteLine("Connecting to {0}", _connection.Socket.RemoteEndPoint);
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                SocketConnection connection = (SocketConnection)ar.AsyncState;

                // Complete the connection.
                connection.Socket.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", connection.Socket.RemoteEndPoint);

                ConnectionState = State.Connected;

                // Signal that the connection has been made.
                connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, 0, OnRecieve, connection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void OnRecieve(IAsyncResult ar)
        {
            SocketConnection connection = (SocketConnection)ar.AsyncState;
            if (connection == null)
            {
                return;
            }
            
            // Update the last message time
            connection.LastMessageTime = DateTime.UtcNow;

            // Read data from the client socket. 
            int bytesRead = connection.Socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                connection.MemoryStream.Write(connection.Buffer, 0, bytesRead);

                if (IsEndOfMessage(connection.Buffer, bytesRead))
                {
                    ISocketMessage message = ReadMessage(connection.MemoryStream);
                    if (message != null)
                    {

                        if (message is PingMessage)
                        {
                            WriteMessage(connection.Socket, new PongMessage((PingMessage)message));
                        }


                        connection.MemoryStream.SetLength(0);

                        // Trigger the callback.
                        OnMessage(message);
                    }
                }
            }

            // Continue Listening
            connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, 0, OnRecieve, connection);
        }

        private void OnUpdate(object state)
        {
            if (ConnectionState == State.Connecting)
            {
                CreateConnection();
            }
            else if (ConnectionState == State.Connected)
            {
                WriteMessage(_connection.Socket, new PingMessage());
            }
        }

        #endregion
    }
}
