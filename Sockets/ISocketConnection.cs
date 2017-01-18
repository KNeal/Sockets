using System;
using System.IO;

namespace Sockets
{
    public interface ISocketConnection
    {
        int ConnectionId { get; }
        string ConnectionName { get; set; }
        DateTime LastMessageTime { get; }
    }
}