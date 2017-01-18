using System;

namespace SocketServer
{
    public interface ISocketConnection
    {
        int ConnectionId { get; }
        string ConnectionName { get; set; }
        DateTime LastMessageTime { get; }
    }
}