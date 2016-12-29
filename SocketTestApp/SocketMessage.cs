using System.IO;

namespace Sockets
{
    public abstract class SocketMessage : ISocketMessage
    {
        public virtual string MessageType
        {
            get { return GetType().Name; }
        }

        public abstract void Serialize(Stream stream);
        public abstract void Deserialize(Stream stream);
    }
}