using System.Collections.Generic;

namespace Sockets
{
    public interface ISocketServer:  ISocketMessageHandler
    {
        void Start(int port);
        void Stop();

        IList<ISocketClient> ConnectedClients { get; }
        
        void SendMessage(int clientId, ISocketMessage message);
        void SendMessageToAllClients(ISocketMessage message, int? excludedClientId = null);
    }
}