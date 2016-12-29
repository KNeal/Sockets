using System;
using System.IO;

namespace Sockets
{
    public interface ISocketMessage
    {
        string MessageType { get; }
        void Serialize(Stream stream);
        void Deserialize(Stream stream);
    }
}