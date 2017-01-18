using System.IO;

namespace SocketServer
{
    public interface ISocketMessage
    {
        string MessageType { get; }
        void Serialize(Stream stream);
        void Deserialize(Stream stream);
    }
}