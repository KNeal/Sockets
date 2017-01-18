using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Sockets.Messages;

namespace Sockets
{
    public abstract class SocketServer : SocketMessageSerializer, ISocketServer
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Socket _socket;
        private readonly Dictionary<int, ClientSocketConnection> _connections = new Dictionary<int, ClientSocketConnection>();

        protected SocketServer()
        {
            RegisterMessageType<PingRequestMessage>("PingRequestMessage", OnPingRequestMessage);
            RegisterMessageType<AuthRequestMessage>("AuthRequestMessage", OnAuthRequestMessage);
        }

        public void Start(int port)
        {
            _lock.EnterWriteLock();
            try
            {
                _socket = OpenSocket(port);

                Console.WriteLine("[SocketServer] Listening at {0}", _socket.LocalEndPoint);

                _socket.BeginAccept(OnSocketAccept, _socket);
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        public void Stop()
        {
            _lock.EnterWriteLock();
            try
            {
                if (_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _socket = null;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        public IList<ISocketConnection> ConnectedClients
        {
            get { return _connections.Values.Select(x => (ISocketConnection)x).ToList(); }
        }

        public void DisconnectClient(int connectionId)
        {
            RemoveConnection(connectionId);
        }

        public void SendMessage(int connectionId, ISocketMessage message)
        {
            ClientSocketConnection client;
            if (_connections.TryGetValue(connectionId, out client))
            {
                if (client.IsAuthenticated)
                {
                    WriteMessage(client, message);  
                }
            }
        }

        public void SendMessageToAllClients(ISocketMessage message, int? excludedConnectionId = null)
        {
            foreach (ClientSocketConnection client in _connections.Values)
            {
                if (client.IsAuthenticated 
                    && (!excludedConnectionId.HasValue || excludedConnectionId.Value != client.ConnectionId))
                {
                    WriteMessage(client, message);           
                }
            }
        }

        protected abstract AuthResult AuthenticateClient(string userName, string userToken);
        protected abstract void OnClientConnected(ISocketConnection client);
        protected abstract void OnClientDisconnected(ISocketConnection client);
        protected abstract void OnMessage(ISocketConnection client, ISocketMessage message);
        
        private void OnAuthRequestMessage(ISocketConnection clientConnection, AuthRequestMessage message)
        {
            AuthResult result = AuthenticateClient(message.UserName, message.UserToken);
            if (result.Success)
            {
                clientConnection.ConnectionName = message.UserName;

                // TODO: Clean up the dependency model instead of doing this hacky cast.
                ((ClientSocketConnection) clientConnection).IsAuthenticated = true;

                OnClientConnected(clientConnection);
            }

            AuthResponseMessage responseMessage = new AuthResponseMessage
            {
                Success = result.Success,
                ErrorMessage = result.Error
            };
            SendMessage(clientConnection.ConnectionId, responseMessage);

            if (!result.Success)
            {
                DisconnectClient(clientConnection.ConnectionId);
            }
        }
        private void OnPingRequestMessage(ISocketConnection client, PingRequestMessage message)
        {
            SendMessage(client.ConnectionId, new PingResponseMessage(message));
        }

        #region Private Methods

        private class ClientSocketConnection : SocketConnection
        {
            public bool IsAuthenticated { get; set; }

            public ClientSocketConnection(Socket socket) 
                : base(socket)
            {
            }
        }

        private Socket OpenSocket(int port)
        {
            // Establish the local endpoint for the socket.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            Socket socket = new Socket(AddressFamily.InterNetwork,  SocketType.Stream, ProtocolType.Tcp );

            // Bind the socket to the local endpoint and listen for incoming connections.
            socket.Bind(localEndPoint);
            socket.Listen(100);

            return socket;
        }

        // TODO... something more robust
        private int clientId = 0;

        private void OnSocketAccept(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket clientSocket = listener.EndAccept(ar);

            Console.WriteLine("[SocketServer] Client connected from at '{0}'", clientSocket.RemoteEndPoint);
            
            // Create the new client
            ClientSocketConnection clientConnection = new ClientSocketConnection(clientSocket)
            {
                ConnectionId = ++clientId
            };
            clientConnection.OnDisconnected += HandleClientDisconnected;
            clientConnection.OnMessage += HandleClientMessage;

            clientConnection.ListenForData();
            
            AddConnection(clientConnection);
        }

        private void AddConnection(ClientSocketConnection clientConnection)
        {
            _lock.EnterWriteLock();
            try
            {
                _connections[clientConnection.ConnectionId] = clientConnection;
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        private void RemoveConnection(int connectionId)
        {
            ClientSocketConnection connection = null;

            _lock.EnterWriteLock();
            try
            {
                if (_connections.TryGetValue(connectionId, out connection))
                {
                    _connections.Remove(connectionId);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            // Kill the socket and notifiy the subclass.
            if (connection != null)
            {
                connection.Disconnect();
                if (connection.IsAuthenticated)
                {
                    OnClientDisconnected(connection);
                }

                connection.OnDisconnected -= HandleClientDisconnected;
                connection.OnMessage -= HandleClientMessage;
            }
        }

        private void HandleClientDisconnected(SocketConnection connection)
        {
            RemoveConnection(connection.ConnectionId);
        }

        private void HandleClientMessage(SocketConnection connection, MemoryStream stream)
        {
            ReadMessage(connection, stream);
        }


        #endregion
    }
}
