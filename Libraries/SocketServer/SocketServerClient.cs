using System;
using System.Collections.Generic;
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

        public string UserName { get; private set; }
        public string Password { get; private set; }
        
        public State ConnectionState { get; private set; }
        public string ConnectionError { get; private set; }

        // Debugging
        public List<ISocketMessage> ReadHistory = new List<ISocketMessage>();

        protected SocketServerClient(string host, int port)
        {
            _host = host;
            _port = port;
            ConnectionState = State.Disconnected;

            RegisterMessageType<PingRequestMessage>("PingRequestMessage", OnPingRequestMessage);
            RegisterMessageType<PingResponseMessage>("PingResponseMessage", OnPingResponseMessage);
            RegisterMessageType<AuthResponseMessage>("AuthResponseMessage", OnAuthResponseMessage);
        }

        public void Dispose()
        {
            Disconnect();
        }

        protected virtual void OnPingRequestMessage(ISocketConnection connect, PingRequestMessage message)
        {
            SendMessage(new PingResponseMessage(message));
        }

        protected virtual void OnPingResponseMessage(ISocketConnection arg1, PingResponseMessage arg2)
        {
           // Do nothing.
        }

        protected virtual void OnAuthResponseMessage(ISocketConnection arg1, AuthResponseMessage arg2)
        {
            ConnectionState = State.Connected;
            OnConnected();
        }

        public void Connect(string userName, string password)
        {
            ConnectionState = State.Connecting;
            ConnectionError = null;
            UserName = userName;
            Password = password;
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

            OnDisconnected();
        }

        public void SendMessage(ISocketMessage message)
        {
            if (_connection != null)
            {
                WriteMessage(_connection, message);
            }
        }

        protected virtual void OnConnected()
        {
            
        }

        protected virtual void OnDisconnected()
        {

        }

        protected virtual void OnMessage(ISocketMessage message)
        {

        }
        
        #region Private Methods
        
        private void OnUpdate(object state)
        {
            if (ConnectionState == State.Connected)
            {
               // WriteMessage(_connection, new PingRequestMessage());
            }
        }

        private void CreateConnection()
        {
            _connection = new SocketConnection();
            _connection.ConnectionName = string.Format("Client:{0}", UserName);
            _connection.OnConnected += HandleConnected;
            _connection.OnDisconnected += HandleDisconnected;
            _connection.OnMessage += HandleMessage;

            _connection.Connect(_host, _port);
        }

        private void HandleMessage(SocketConnection connection, MemoryStream stream)
        {
            ISocketMessage message = ReadMessage(connection, stream);
            ReadHistory.Add(message);
            OnMessage(message);
        }

        private void HandleConnected(SocketConnection connection)
        {
            ConnectionState = State.Authenticating;
            SendMessage(new AuthRequestMessage() {UserName = UserName, UserToken = Password});
        }

        private void HandleDisconnected(SocketConnection connection)
        {
            Disconnect();
        }

        #endregion
    }
}
