using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Microsoft.SqlServer.Server;

namespace Sockets
{
    /// <summary>
    /// This is a base class for serializing and deserializing socket messages.
    /// </summary>
    public abstract class SocketMessageHandler : ISocketMessageHandler
    {
        public static readonly byte[] EndOfMessageBytes = Encoding.ASCII.GetBytes("[EOM]");

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<string, Type> _messageTypes = new Dictionary<string, Type>();

        public SocketMessageHandler()
        {
            RegisterMessageType<PingMessage>("PingMessage");
            RegisterMessageType<PongMessage>("PongMessage");
        }

        public void RegisterMessageType<T>(string messageId)
        {
            _lock.EnterWriteLock();
            try
            {
                _messageTypes[messageId] = typeof (T);
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        protected void WriteMessage(Socket socket, ISocketMessage message)
        {
            if (socket != null)
            {
                // TODO: pool use of memory streams.
                MemoryStream stream = new MemoryStream();

                // Write the Header
                BinaryUtils.WriteString(stream, message.MessageType);

                // Write the Data
                message.Serialize(stream);

                // Write the Footer
                stream.Write(EndOfMessageBytes, 0, EndOfMessageBytes.Length);
                socket.Send(stream.ToArray());
            }
        }

        protected ISocketMessage ReadMessage(MemoryStream stream)
        {
            try
            {
                // Verify the stream has the EOM bytes
                stream.Seek(-EndOfMessageBytes.Length, SeekOrigin.End);
                for (int i = 0; i < EndOfMessageBytes.Length; ++i)
                {
                    if (stream.ReadByte() != EndOfMessageBytes[i])
                    {
                        return null;
                    }
                }

                // Strip off the Footer Data
                stream.SetLength(stream.Length - EndOfMessageBytes.Length);
                
                // Read the Header
                stream.Seek(0, 0);
                string messageTypeName = BinaryUtils.ReadString(stream);
                Type messageType;
                if (!_messageTypes.TryGetValue(messageTypeName, out messageType))
                {
                    Console.WriteLine("[MessageSocketBase] Failed to find message type: {0}", messageTypeName);
                    return null;
                }
                
                // Read the data
                ISocketMessage message = Activator.CreateInstance(messageType) as ISocketMessage;
                if (message == null)
                {
                    Console.WriteLine("[MessageSocketBase] Failed to create message type: {0}", messageTypeName);
                    return null;
                }
                
                message.Deserialize(stream);
                return message;
            }
            catch (Exception e)
            {
                Console.WriteLine("[MessageSocketBase] Failed to deserialize message: {0}", e.Message);
                return null;
            }
        }

        protected bool IsEndOfMessage(byte[] bytes, int len)
        {
            if (len < EndOfMessageBytes.Length)
            {
                return false;
            }

            // Compare the last bytes in the data array to the expected end of array.
            for (int i = 0; i < EndOfMessageBytes.Length; ++i)
            {
                if (bytes[len - 1 - i] != EndOfMessageBytes[EndOfMessageBytes.Length - 1 - i])
                {
                    return false;
                }
            }

            return true;
        }

        private byte[] ReadBytes(Stream stream, int count)
        {
            byte[] bytes = new byte[count];
            for (int i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = (byte)stream.ReadByte();
            }

            return bytes;
        }

    }
}