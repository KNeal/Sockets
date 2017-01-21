using System;
using System.IO;
using SocketServer.Messages;
using SocketServer.Utils;

namespace Calculator.Messages
{
    [Serializable]
    public class CalculationErrorMessage : SocketMessage
    {
        public string Message { get; set; }

        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteString(stream, Message);
        }

        public override void Deserialize(Stream stream)
        {
            Message = BinaryUtils.ReadString(stream);
        }
    }
}