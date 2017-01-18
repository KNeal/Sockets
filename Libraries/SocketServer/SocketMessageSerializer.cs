using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SocketServer.Utils;

namespace SocketServer
{
    /// <summary>
    /// This is a base class for serializing and deserializing socket messages.
    /// </summary>
    public abstract class SocketMessageSerializer
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<string, MessageTypeHandler> _messageTypes = new Dictionary<string, MessageTypeHandler>();

        protected void RegisterMessageType<T>(string messageId, Action<ISocketConnection, T> messageHandler) where T : ISocketMessage
        {
            _lock.EnterWriteLock();
            try
            {
                MessageTypeHandler handler;
                if (!_messageTypes.TryGetValue(messageId, out handler))
                {
                    handler = new MessageTypeHandler<T>();
                    _messageTypes.Add(messageId, handler);
                }

                MessageTypeHandler<T> handlerT = handler as MessageTypeHandler<T>;
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

        protected void ReadMessage(SocketConnection connection, Stream stream)
        {
            _lock.EnterReadLock();
            try
            {
                // Read the Header
                string messageTypeName = BinaryUtils.ReadString(stream);

                // Read the Message
                MessageTypeHandler messageTypeHandler;
                if (_messageTypes.TryGetValue(messageTypeName, out messageTypeHandler))
                {
                    messageTypeHandler.ReadMessage(connection, stream);
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
            finally
            {
                _lock.ExitReadLock();
            }
        }

        protected void WriteMessage(SocketConnection connection, ISocketMessage message)
        {
            connection.WriteMessage((stream) =>
            {
                BinaryUtils.WriteString(stream, message.MessageType);
                message.Serialize(stream);
            });
        }

        #region Private Classes

        private abstract class MessageTypeHandler
        {
            public abstract void ReadMessage(ISocketConnection connection, Stream stream);
        }

        private class MessageTypeHandler<T> : MessageTypeHandler where T : ISocketMessage
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

            public override void ReadMessage(ISocketConnection connection, Stream stream)
            {
                ISocketMessage message = Activator.CreateInstance(_type) as ISocketMessage;
                if (message == null)
                {
                    Console.WriteLine("[SocketMessageHandler] Failed to create message of type={0}", _type);
                    return;
                }

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
            }
        }
        #endregion
    }
}