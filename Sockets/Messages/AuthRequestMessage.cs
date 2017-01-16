using System.IO;

namespace Sockets.Messages
{
    public class AuthRequestMessage : SocketMessage
    {
        public string UserName { get; set; }
        public string UserToken { get; set; }


        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteString(stream, UserName);
            BinaryUtils.WriteString(stream, UserToken);
        }

        public override void Deserialize(Stream stream)
        {
            UserName = BinaryUtils.ReadString(stream);
            UserToken = BinaryUtils.ReadString(stream);
        }
    }
}