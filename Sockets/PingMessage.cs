using System;
using System.IO;

namespace Sockets
{
    public class PingMessage : SocketMessage
    {
        public DateTime SendTimeUtc { get; private set; }

        public PingMessage()
        {
            SendTimeUtc = DateTime.UtcNow;
        }

        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteInt64(stream, SendTimeUtc.Ticks);
        }

        public override void Deserialize(Stream stream)
        {
            long ticks = BinaryUtils.ReadInt64(stream);
            SendTimeUtc = new DateTime(ticks);
        }
    }
}