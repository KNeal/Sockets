using System;
using System.IO;
using Sockets;

namespace Calculator
{
    [Serializable]
    public class CalculationResultMessage : SocketMessage
    {
        public int Value { get; set; }
        
        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteInt(stream, Value);
        }

        public override void Deserialize(Stream stream)
        {
            Value = BinaryUtils.ReadInt(stream);
        }
    }
}