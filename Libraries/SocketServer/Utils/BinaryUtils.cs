using System;
using System.IO;
using System.Text;

namespace SocketServer.Utils
{
    public static class BinaryUtils
    {
        public static void WriteBool(Stream stream, bool value)
        {
            byte[] b = {value ? (byte) 1 : (byte) 0};
            stream.Write(b, 0, 1);
        }

        public static bool ReadBool(Stream stream)
        {
            int b = stream.ReadByte();
            return b > 0;
        }

        public static void WriteInt32(Stream stream, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static int ReadInt32(Stream stream)
        {
            byte[] bytes = new byte[sizeof(Int32)];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = (byte)stream.ReadByte();
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        public static void WriteInt64(Stream stream, long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static long ReadInt64(Stream stream)
        {
            byte[] bytes = new byte[sizeof(Int64)];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = (byte)stream.ReadByte();
            }

            return BitConverter.ToInt64(bytes, 0);
        }

        public static void WriteUInt32(Stream stream, UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static UInt32 ReadUInt32(Stream stream)
        {
            byte[] bytes = new byte[sizeof(UInt32)];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = (byte)stream.ReadByte();
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static void WriteUInt64(Stream stream, UInt64 value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static UInt64 ReadUInt64(Stream stream)
        {
            byte[] bytes = new byte[sizeof(UInt64)];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = (byte)stream.ReadByte();
            }

            return BitConverter.ToUInt64(bytes, 0);
        }

        public static void WriteString(Stream stream, string value)
        {
            if (value == null)
            {
                WriteInt32(stream, 0);
            }
            else
            {
                byte[] data = Encoding.UTF8.GetBytes(value);

                WriteInt32(stream, data.Length);
                stream.Write(data, 0, data.Length);
            }
        }

        public static string ReadString(Stream stream)
        {
            int len = ReadInt32(stream);

            if (len == 0)
            {
                return null;
            }
            else
            {
                byte[] dataBytes = new byte[len];
                for (int i = 0; i < len; ++i)
                {
                    dataBytes[i] = (byte)stream.ReadByte();
                }

                return Encoding.UTF8.GetString(dataBytes);
            }
        }
    }
}