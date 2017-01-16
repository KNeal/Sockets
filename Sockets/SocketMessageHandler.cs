using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Sockets.Messages;

namespace Sockets
{
    /// <summary>
    /// This is a base class for serializing and deserializing socket messages.
    /// </summary>
    public abstract class SocketMessageHandler : ISocketMessageHandler
    {
        public static readonly byte[] EndOfMessageBytes = Encoding.ASCII.GetBytes("[EOM]");
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<string, MessageTypeHandler> _messageTypes = new Dictionary<string, MessageTypeHandler>();
        
        public void RegisterMessageType<T>(string messageId, Action<ISocketConnection, T> messageHandler) where T : ISocketMessage
        {
            _lock.EnterWriteLock();
            try
            {
                MessageTypeHandler handler;
                if (_messageTypes.TryGetValue(messageId, out handler))
                {
                    handler = new MessageTypeHandler<T>();
                    _messageTypes.Add(messageId, handler);
                }

                MessageTypeHandler<ISocketConnection, T> handlerT = handler as MessageTypeHandler<ISocketConnection, T>;
                if (handlerT != null)
                {
                    handlerT.RegisterHandler(messageHandler);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }    
        }

        public void WriteMessage(Socket socket, ISocketMessage message)
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

        public ISocketMessage ReadMessage(MemoryStream stream)
        {
            ISocketMessage message = null;
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
                MessageTypeHandler messageTypeHandler;
                if (_messageTypes.TryGetValue(messageTypeName, out messageTypeHandler))
                {
                    message = messageTypeHandler.ReadMessage(stream);
                }
                else
                {
                    Console.WriteLine("[MessageSocketBase] Failed to find message type: {0}", messageTypeName);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("[MessageSocketBase] Failed to deserialize message: {0}", e.Message);
            }

            return message;
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

        #region Private Classes


        private abstract class MessageTypeHandler
        {
            public abstract ISocketMessage ReadMessage(ISocketConnection connection, MemoryStream stream);
        }

        private class MessageTypeHandler<ISocketConnection,T> : MessageTypeHandler where T : ISocketMessage
        {
            private readonly Type _type = typeof(T);
            private readonly List<Action<ISocketConnection, T>> _messageHandlers = new List<Action<ISocketConnection,T>>();

            public void RegisterHandler(Action<ISocketConnection,T> messageHander)
            {
                if (messageHander != null)
                {
                    // TODO... handle locks?
                    _messageHandlers.Add(messageHander);
                }
            }

            public override ISocketMessage ReadMessage(ISocketConnection connection, MemoryStream stream)
            {
                ISocketMessage message = Activator.CreateInstance(_type) as ISocketMessage;
                message.Deserialize(stream);

                foreach (Action<ISocketConnection,T> messageHandler in _messageHandlers)
                {
                    try
                    {
                        messageHandler(connection, (T)message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[SocketMessageHandler] Process failure: {0}", e);
                    }
                }

                return message;
            }

        }
        #endregion

    }
}