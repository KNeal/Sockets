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

            RegisterMessageType<PingRequestMessage>("PingRequestMessage", OnPingRequestMessage);
        }

        private void OnPingRequestMessage(ISocketConnection connect, PingRequestMessage message)
        {
            SendMessage(new PingResponseMessage(message));
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
                    _connection.Disconnect();
                    _connection.Disconnect();
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
            _connection.WriteMessage(message);
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
                _connection.WriteMessage(new PingRequestMessage());
            }
        }

        private void CreateConnection()
        {
            _connection = new SocketConnection(this);
            _connection.OnConnected += delegate(SocketConnection connection)
            {
                SendMessage(new AuthRequestMessage() {UserName = _userName, UserToken = _password});
            };
            _connection.Connect(_host, _port);
        }

       


        #endregion
    }
}
