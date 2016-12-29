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
        private Dictionary<int, Client> _clients = new Dictionary<int, Client>(); 
        
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
            Client client;
            if (_clients.TryGetValue(clientId, out client))
            {
                WriteMessage(client.Socket, message);
            }
        }

        public void SendMessageToAllClients(ISocketMessage message)
        {
            foreach (Client client in _clients.Values)
            {
                WriteMessage(client.Socket, message);
            }
        }

        protected abstract void OnClientConnected(ISocketClient client);
        protected abstract void OnClientDisconnected(ISocketClient client);
        protected abstract void OnMessage(int clientId, ISocketMessage message);

        #region Private Classes

        private const int ClientReadBufferLen = 1024;
        public class Client : ISocketClient
        {
            public Socket Socket;
            public byte[] ReadBuffer = new byte[ClientReadBufferLen];
            public MemoryStream ReadMemoryStream = new MemoryStream();

            public int ClientId { get; set; }

            public string ClientName { get; set; }

            public DateTime LastActiveTime { get; set; }
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
            Client client = new Client()
            {
                Socket = clientSocket,
                ClientId = clientId++
            };
            client.Socket.BeginReceive(client.ReadBuffer, 0, client.ReadBuffer.Length, 0,
                OnClientRecieve, client);

             _lock.EnterWriteLock();
            
            AddClient(client);
            OnClientConnected(client);
        }

        public void OnClientRecieve(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            if (client == null)
            {
                return;
            }
            
            // Update the last message time
            client.LastActiveTime = DateTime.UtcNow;

            // Read data from the client socket. 
            int bytesRead = client.Socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                client.ReadMemoryStream.Write(client.ReadBuffer, 0, bytesRead);

                if (IsEndOfMessage(client.ReadBuffer, bytesRead))
                {
                    ISocketMessage message = ReadMessage(client.ReadMemoryStream);
                    client.ReadMemoryStream.SetLength(0);

                    // Trigger the callback.
                    if (message != null)
                    {
                        OnMessage(client.ClientId, message);
                    }
                }
            }

            // Continue Listening
            client.Socket.BeginReceive(client.ReadBuffer, 0, client.ReadBuffer.Length, 0,
                OnClientRecieve, client);
        }

        private void AddClient(Client client)
        {
            _lock.EnterWriteLock();
            try
            {
                _clients[client.ClientId] = client;
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        #endregion
    }
}
