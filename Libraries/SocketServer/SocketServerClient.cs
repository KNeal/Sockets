using System;
using System.IO;
using System.Threading;
using SocketServer.Messages;

namespace SocketServer
{
    public abstract class SocketServerClient : SocketMessageSerializer, IDisposable
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

        protected SocketServerClient(string host, int port)
        {
            _host = host;
            _port = port;
            ConnectionState = State.Disconnected;

            RegisterMessageType<PingRequestMessage>("PingRequestMessage", OnPingRequestMessage);
            RegisterMessageType<AuthResponseMessage>("AuthResponseMessage", OnAuthResponseMessage);
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void OnPingRequestMessage(ISocketConnection connect, PingRequestMessage message)
        {
            SendMessage(new PingResponseMessage(message));
        }

        private void OnAuthResponseMessage(ISocketConnection arg1, AuthResponseMessage arg2)
        {
            ConnectionState = State.Connected;
        }

        public void Connect(string userName, string password)
        {
            ConnectionState = State.Connecting;
            ConnectionError = null;
            _userName = userName;
            _password = password;
            _updateTimer =  new Timer(OnUpdate, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            CreateConnection();
        }

        public void Disconnect()
        {
            _lock.EnterWriteLock();
            try
            {
                if (_connection != null)
                {
                    _connection.Disconnect();
                    _connection.OnConnected -= HandleConnected;
                    _connection.OnDisconnected -= HandleDisconnected;
                    _connection.OnMessage += HandleMessage;
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
            if (_connection != null)
            {
                WriteMessage(_connection, message);
            }
        }

        protected virtual void OnMessage(ISocketMessage message)
        {
            
        }
        
        #region Private Methods
        
        private void OnUpdate(object state)
        {
            if (ConnectionState == State.Connected)
            {
                WriteMessage(_connection, new PingRequestMessage());
            }
        }

        private void CreateConnection()
        {
            _connection = new SocketConnection();
            _connection.OnConnected += HandleConnected;
            _connection.OnDisconnected += HandleDisconnected;
            _connection.OnMessage += HandleMessage;

            _connection.Connect(_host, _port);
        }

        private void HandleMessage(SocketConnection connection, MemoryStream stream)
        {
            ReadMessage(connection, stream);
        }

        private void HandleConnected(SocketConnection connection)
        {
            ConnectionState = State.Authenticating;
            SendMessage(new AuthRequestMessage() {UserName = _userName, UserToken = _password});
        }

        private void HandleDisconnected(SocketConnection connection)
        {
            Disconnect();
        }

        #endregion
    }
}
