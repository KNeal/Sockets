using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Sockets.Messages;

namespace Sockets
{
    public abstract class SocketServer : SocketMessageHandler, ISocketServer
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Socket _socket;
        private readonly Dictionary<int, ClientConnection> _connections = new Dictionary<int, ClientConnection>(); 
        
        public void Start(int port)
        {
            _lock.EnterWriteLock();
            try
            {
                _socket = OpenSocket(port);

                Console.WriteLine("Opening socket at {0}", _socket.LocalEndPoint);

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

        public IList<ISocketClient> ConnectedClients
        {
            get { return _connections.Values.Select(x => (ISocketClient)x).ToList(); }
        }

        public void SendMessage(int connectionId, ISocketMessage message)
        {
            ClientConnection clientConnection;
            if (_connections.TryGetValue(connectionId, out clientConnection))
            {
                if (clientConnection.IsAuthenticated)
                {
                    WriteMessage(clientConnection.Socket, message);   
                }
            }
        }

        public void SendMessageToAllClients(ISocketMessage message, int? excludedConnectionId = null)
        {
            foreach (ClientConnection client in _connections.Values)
            {
                if (client.IsAuthenticated 
                    && (!excludedConnectionId.HasValue || excludedConnectionId.Value != client.ConnectionId))
                {
                    WriteMessage(client.Socket, message);
                }
            }
        }

        protected abstract AuthResult AuthenticateClient(string userName, string userToken);
        protected abstract void OnClientConnected(ISocketClient client);
        protected abstract void OnClientDisconnected(ISocketClient client);
        protected abstract void OnMessage(ISocketClient client, ISocketMessage message);

        #region Private Classes

        private class ClientConnection : SocketConnection, ISocketClient
        {
            public int ConnectionId { get; set; }
            public bool IsAuthenticated { get; set; }
            public string UserName { get; set; }

            public ClientConnection(Socket socket, ISocketMessageHandler messageHandler, int bufferLen = DefaultBufferLen)
                : base(socket, messageHandler, bufferLen)
            {
             
            }
        }

        #endregion

        #region Private Methods

        private Socket OpenSocket(int port)
        {
            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
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

        private int clientId = 0;

        public void OnSocketAccept(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket clientSocket = listener.EndAccept(ar);

            Console.WriteLine("Client connected from at {0}", clientSocket.RemoteEndPoint);
            
            // Create the new client
            ClientConnection clientConnection = new ClientConnection(clientSocket, this)
            {
                ConnectionId = clientId++
            };
            clientConnection.ListenForData();
            
            AddConnection(clientConnection);
        }

        private void AddConnection(ClientConnection clientConnection)
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

        private void RemoveConnection(ClientConnection clientConnection)
        {

            _lock.EnterWriteLock();
            try
            {
                if (_connections.ContainsKey(clientConnection.ConnectionId))
                {
                    _connections.Remove(clientConnection.ConnectionId);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            // Kill the socket and notifiy the subclass.
            clientConnection.Close();
            if (clientConnection.IsAuthenticated)
            {
                OnClientDisconnected(clientConnection);
            }
        }

        private bool HandleMessage(ClientConnection clientConnection, ISocketMessage message)
        {
            if (message is AuthRequestMessage)
            {
                return OnAuthenticateRequestMessage(clientConnection, (message as AuthRequestMessage));
            }
            else if (message is PingRequestMessage)
            {
                WriteMessage(clientConnection.Socket, new PingResponseMessage((PingRequestMessage)message));
            }
            else if(clientConnection.IsAuthenticated)
            {
                // Only forward messages for authenticated clients.
                OnMessage(clientConnection, message);
            }

            return true;
        }

        private bool OnAuthenticateRequestMessage(ClientConnection clientConnection, AuthRequestMessage message)
        {
            AuthResult result = AuthenticateClient(message.UserName, message.UserToken);
            if (result.Success)
            {
                clientConnection.UserName = message.UserName;
                OnClientConnected(clientConnection);
            }
            else
            {
                // Kill the connection
                clientConnection.Close();
                RemoveConnection(clientConnection);
            }

            AuthResponseMessage responseMessage = new AuthResponseMessage
            {
                Success = result.Success,
                ErrorMessage = result.Error
            };
            WriteMessage(clientConnection.Socket, responseMessage);

            return result.Success;
        }

        #endregion
    }
}
