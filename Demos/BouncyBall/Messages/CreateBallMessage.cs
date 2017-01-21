using System;
using System.IO;
using SocketServer.Messages;
using SocketServer.Utils;

namespace BouncingBalls.Messages
{
    [Serializable]
    public class CreateBallMessage : SocketMessage
    {
        public string BallId { get; set; }
        public string Color { get; set; }
        public int Radius { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }

        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteString(stream, BallId);
            BinaryUtils.WriteString(stream, Color);
            BinaryUtils.WriteInt32(stream, Radius);
            BinaryUtils.WriteInt32(stream, XPos);
            BinaryUtils.WriteInt32(stream, YPos);
        }

        public override void Deserialize(Stream stream)
        {
            BallId = BinaryUtils.ReadString(stream);
            Color = BinaryUtils.ReadString(stream);
            Radius = BinaryUtils.ReadInt32(stream);
            XPos = BinaryUtils.ReadInt32(stream);
            YPos = BinaryUtils.ReadInt32(stream);
        }
    }
}