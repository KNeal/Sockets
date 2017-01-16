using System.Security.Policy;

namespace Sockets
{
    public interface ISocketClient
    {
        int ConnectionId { get; }
        string UserName { get; }
    }
}