using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sockets
{
    public abstract class SocketServer : SocketMessageHandler, ISocketServer
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Socket _socket;
        private Dictionary<int, Connection> _clients = new Dictionary<int, Connection>(); 
        
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
            get { return _clients.Values.Select(x => (ISocketClient)x).ToList(); }
        }

        public void SendMessage(int clientId, ISocketMessage message)
        {
            Connection connection;
            if (_clients.TryGetValue(clientId, out connection))
            {
                WriteMessage(connection.Socket, message);
            }
        }

        public void SendMessageToAllClients(ISocketMessage message)
        {
            foreach (Connection client in _clients.Values)
            {
                WriteMessage(client.Socket, message);
            }
        }

        protected abstract void OnClientConnected(ISocketClient client);
        protected abstract void OnClientDisconnected(ISocketClient client);
        protected abstract void OnMessage(int clientId, ISocketMessage message);

        #region Private Classes

        private const int ClientReadBufferLen = 1024;
        public class Connection : SocketConnection, ISocketClient
        {
            public int ClientId { get; set; }
            public string ClientName { get; set; }

            public Connection(Socket socket, int bufferLen = DefaultBufferLen) : base(socket, bufferLen)
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
            Connection connection = new Connection(clientSocket)
            {
                ClientId = clientId++
            };
            connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, 0, OnClientRecieve, connection);

             _lock.EnterWriteLock();
            
            AddClient(connection);
            OnClientConnected(connection);
        }

        public void OnClientRecieve(IAsyncResult ar)
        {
            Connection connection = (Connection)ar.AsyncState;
            if (connection == null)
            {
                return;
            }

            try
            {
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
                        connection.MemoryStream.SetLength(0);

                        // Trigger the callback.
                        if (message != null)
                        {
                            if (message is PingMessage)
                            {
                                WriteMessage(connection.Socket, new PongMessage((PingMessage)message));    
                            }

                            OnMessage(connection.ClientId, message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[SocketServer] Error in recieving message: {0}", e.Message);
            }

            // Continue Listening
            connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, 0, OnClientRecieve, connection);
        }

        private void AddClient(Connection connection)
        {
            _lock.EnterWriteLock();
            try
            {
                _clients[connection.ClientId] = connection;
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        #endregion
    }
}
