using System;
using System.IO;

namespace SocketServer
{
    public interface ISocketMessage
    {
        UInt64 MessageId { get; set; }
        string MessageType { get; }
        void Serialize(Stream stream);
        void Deserialize(Stream stream);
    }
}