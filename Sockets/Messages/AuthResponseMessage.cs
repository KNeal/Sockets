using System.IO;

namespace Sockets.Messages
{
    public class AuthResponseMessage : SocketMessage
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteBool(stream, Success);
            BinaryUtils.WriteString(stream, ErrorMessage);
        }

        public override void Deserialize(Stream stream)
        {
            Success = BinaryUtils.ReadBool(stream);
            ErrorMessage = BinaryUtils.ReadString(stream);
        }
    }
}