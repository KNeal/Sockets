using System;

namespace Sockets
{
    public interface ISocketConnection
    {
        int ConnectionId { get; }
        string ConnectionName { get; set; }
        DateTime LastMessageTime { get; }

        void Disconnect();
    }
}