using System.IO;
using SocketServer.Utils;

namespace SocketServer.Messages
{
    public class DisconnectMessage : SocketMessage
    {
        public string Reason { get; set; }
        
        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteString(stream, Reason);
        }

        public override void Deserialize(Stream stream)
        {
            Reason = BinaryUtils.ReadString(stream);
        }
    }
}