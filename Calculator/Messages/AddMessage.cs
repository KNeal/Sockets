﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sockets;

namespace Calculator
{
    [Serializable]
    public class AddMessage : SocketMessage
    {
        public string Id { get { return "Add"; } }
        public int Value1 { get; set; }
        public int Value2 { get; set; }

        public override void Serialize(Stream stream)
        {
            BinaryUtils.WriteInt(stream, Value1);
            BinaryUtils.WriteInt(stream, Value2);
        }

        public override void Deserialize(Stream stream)
        {
            Value1 = BinaryUtils.ReadInt(stream);
            Value2 = BinaryUtils.ReadInt(stream);
        }
    }
}