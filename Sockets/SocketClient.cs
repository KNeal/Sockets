using System;
using System.Net.Sockets;
using System.Threading;
using Sockets.Messages;

namespace Sockets
{
    public abstract class SocketClient : SocketMessageHandler
    {
        public enum State
        {
            Disconnected,
            Connecting,
            Authenticating,
            Connected,
            Error
        }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly string _host;
        private readonly int _port;
        private SocketConnection _connection;
        private Timer _updateTimer;

        private string _userName;
        private string _password;
        
        public State ConnectionState { get; private set; }
        public string ConnectionError { get; private set; }

        protected SocketClient(string host, int port)
        {
            _host = host;
            _port = port;
            ConnectionState = State.Disconnected;

            RegisterMessageType<PingResponseMessage>("PingResponseMessage", OnPingResponse);
        }

        private void OnPingResponse(PingResponseMessage message)
        {
            throw new NotImplementedException();
        }

        public void Connect(string userName, string password)
        {
            ConnectionState = State.Connecting;
            ConnectionError = null;
            _userName = userName;
            _password = password;
            _updateTimer =  new Timer(OnUpdate, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public void Disconnect()
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
        
        private void OnUpdate(object state)
        {
            if (ConnectionState == State.Connecting)
            {
                CreateConnection();
            }
            else if (ConnectionState == State.Connected)
            {
                WriteMessage(_connection.Socket, new PingRequestMessage());
            }
        }
        private void CreateConnection()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _connection = new SocketConnection(socket, this);
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
                connection.ListenForData();

                SendMessage(new AuthRequestMessage() {UserName = _userName, UserToken = _password});
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        #endregion
    }
}
