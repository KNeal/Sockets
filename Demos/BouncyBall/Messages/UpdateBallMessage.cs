using System;
using System.IO;
using SocketServer.Messages;
using SocketServer.Utils;

namespace BouncingBalls.Messages
{
    [Serializable]
    public class UpdateBallMessage : SocketMessage
    {
        public string BallId { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }

        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteString(stream, BallId);
            BinaryUtils.WriteInt32(stream, XPos);
            BinaryUtils.WriteInt32(stream, YPos);
        }

        public override void Deserialize(Stream stream)
        {
            BallId = BinaryUtils.ReadString(stream);
            XPos = BinaryUtils.ReadInt32(stream);
            YPos = BinaryUtils.ReadInt32(stream);
        }
    }
}
