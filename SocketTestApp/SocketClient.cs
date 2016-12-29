using System;
using System.Net.Sockets;
using System.Threading;

namespace Sockets
{
    public interface ISocketClient
    {
        
    }

    public abstract class SocketClient : SocketMessageHandler
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private SocketState _socket;

        public bool IsConnected;
        
        public void Start(string host, int port)
        {
            _lock.EnterWriteLock();
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket = new SocketState(socket);
                _socket.Socket.BeginConnect(host, port, OnConnect, _socket);
                Console.WriteLine("Connecting to {0}", _socket.Socket.RemoteEndPoint);

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
                    _socket.Close();
                    _socket.Close();
                    _socket = null;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        public void SendMessage(ISocketMessage message)
        {
            WriteMessage(_socket.Socket, message);
        }

        protected abstract void OnMessage(ISocketMessage message);
        
        #region Private Methods

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                SocketState state = (SocketState)ar.AsyncState;

                // Complete the connection.
                state.Socket.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", state.Socket.RemoteEndPoint);

                // Signal that the connection has been made.
                state.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnRecieve, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void OnRecieve(IAsyncResult ar)
        {
            SocketState state = (SocketState)ar.AsyncState;
            if (state == null)
            {
                return;
            }
            
            // Update the last message time
            state.LastMessageTime = DateTime.UtcNow;

            // Read data from the client socket. 
            int bytesRead = state.Socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.MemoryStream.Write(state.Buffer, 0, bytesRead);

                if (IsEndOfMessage(state.Buffer, bytesRead))
                {
                    ISocketMessage message = ReadMessage(state.MemoryStream);
                    state.MemoryStream.SetLength(0);

                    // Trigger the callback.
                    OnMessage(message);
                }
            }

            // Continue Listening
            state.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnRecieve, state);
        }

        #endregion
    }
}
