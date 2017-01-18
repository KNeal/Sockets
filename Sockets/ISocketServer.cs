using System.Collections.Generic;

namespace Sockets
{
    public interface ISocketServer
    {
        void Start(int port);
        void Stop();

        IList<ISocketConnection> ConnectedClients { get; }
        
        void SendMessage(int clientId, ISocketMessage message);
        void SendMessageToAllClients(ISocketMessage message, int? excludedClientId = null);
    }
}