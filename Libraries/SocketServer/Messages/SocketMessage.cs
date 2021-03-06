using System;
using System.IO;

namespace SocketServer.Messages
{
    public abstract class SocketMessage : ISocketMessage
    {
        public UInt64 MessageId { get; set; }

        public virtual string MessageType
        {
            get { return GetType().Name; }
        }

        public abstract void Serialize(Stream stream);
        public abstract void Deserialize(Stream stream);
    }
}