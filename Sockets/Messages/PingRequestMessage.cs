using System;
using System.IO;

namespace Sockets.Messages
{
    public class PingRequestMessage : SocketMessage
    {
        public DateTime SendTimeUtc { get; private set; }

        public PingRequestMessage()
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