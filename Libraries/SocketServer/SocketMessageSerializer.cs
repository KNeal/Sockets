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

        public void RegisterMessageType<T>(string messageId, Action<ISocketConnection, T> messageHandler) where T : ISocketMessage
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

        public ISocketMessage ReadMessage(SocketConnection connection, Stream stream)
        {
            _lock.EnterReadLock();
            try
            {
                // Read the Header
                UInt64 messageId = BinaryUtils.ReadUInt64(stream);
                string messageTypeName = BinaryUtils.ReadString(stream);

                // Read the Message
                MessageTypeHandler messageTypeHandler;
                if (_messageTypes.TryGetValue(messageTypeName, out messageTypeHandler))
                {
                    return messageTypeHandler.ReadMessage(connection, stream, messageId);
                }
                else
                {
                    Logger.Info("[MessageSocketBase] Failed to find message type: {0}", messageTypeName);
                }

            }
            catch (Exception e)
            {
                Logger.Error("[MessageSocketBase] Failed to deserialize message: {0}", e.Message);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return null;
        }

        public virtual void WriteMessage(SocketConnection connection, ISocketMessage message)
        {
            connection.WriteMessage((stream) =>
            {
                BinaryUtils.WriteUInt64(stream, message.MessageId);
                BinaryUtils.WriteString(stream, message.MessageType);
                message.Serialize(stream);
            });
        }

        #region Private Classes

        private abstract class MessageTypeHandler
        {
            public abstract ISocketMessage ReadMessage(ISocketConnection connection, Stream stream, UInt64 messageId);
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

            public override ISocketMessage ReadMessage(ISocketConnection connection, Stream stream, UInt64 messageId)
            {
                ISocketMessage message = Activator.CreateInstance(_type) as ISocketMessage;
                if (message == null)
                {
                    Logger.Error("[SocketMessageHandler] Failed to create message of type={0}", _type);
                    return null;
                }
                message.MessageId = messageId;
                message.Deserialize(stream);

                foreach (Action<ISocketConnection,T> messageHandler in _messageHandlers)
                {
                    try
                    {
                        messageHandler(connection, (T)message);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("[SocketMessageHandler] Process failure: {0}", e);
                    }
                }

                return message;                
            }
        }
        #endregion
    }
}