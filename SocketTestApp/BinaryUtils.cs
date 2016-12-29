using System;
using System.IO;
using System.Text;

namespace Sockets
{
    public static class BinaryUtils
    {
        public static void WriteInt(Stream stream, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static int ReadInt(Stream stream)
        {
            byte[] bytes = new byte[sizeof(Int32)];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = (byte)stream.ReadByte();
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        public static void WriteString(Stream stream, string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);

            WriteInt(stream, data.Length);
            stream.Write(data, 0, data.Length);
        }

        public static string ReadString(Stream stream)
        {
            int len = ReadInt(stream);

            byte[] dataBytes = new byte[len];
            for (int i = 0; i < len; ++i)
            {
                dataBytes[i] = (byte)stream.ReadByte();
            }

            return Encoding.UTF8.GetString(dataBytes);
        }
    }
}