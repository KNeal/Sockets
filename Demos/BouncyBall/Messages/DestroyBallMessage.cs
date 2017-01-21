using System;
using System.IO;
using SocketServer.Messages;
using SocketServer.Utils;

namespace BouncingBalls.Messages
{
    [Serializable]
    public class DestroyBallMessage : SocketMessage
    {
        public string BallId { get; set; }

        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteString(stream, BallId);
        }

        public override void Deserialize(Stream stream)
        {
            BallId = BinaryUtils.ReadString(stream);
        }
    }
}