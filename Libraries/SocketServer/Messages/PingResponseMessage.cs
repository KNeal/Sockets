using System;
using System.IO;
using SocketServer.Utils;

namespace SocketServer.Messages
{
    public class PingResponseMessage : SocketMessage
    {
        public DateTime SendTimeUtc { get; private set; }
        public DateTime RecieveTimeUtc { get; private set; }

        public TimeSpan ElapsedTime { get { return RecieveTimeUtc - SendTimeUtc; } }

        public PingResponseMessage()
        {

        }

        public PingResponseMessage(PingRequestMessage pingRequest)
        {
            SendTimeUtc = pingRequest.SendTimeUtc;
        }

        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteInt64(stream, SendTimeUtc.Ticks);
        }

        public override void Deserialize(Stream stream)
        {
            long ticks = BinaryUtils.ReadInt64(stream);
            SendTimeUtc = new DateTime(ticks);
            RecieveTimeUtc = DateTime.UtcNow; // Record the client time received
        }
    }
}