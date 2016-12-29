using System;

namespace Sockets
{
    public interface ISocketConnection
    {
        int ClientId { get; }
        string ClientName { get; }
        DateTime LastActiveTime { get; }
    }
}