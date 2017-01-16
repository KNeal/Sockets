using System;
using System.IO;
using System.Net.Sockets;

namespace Sockets
{
    public interface ISocketMessageHandler
    {
        void RegisterMessageType<T>(string messageId, Action<ISocketConnection, T> messageHandler) where T : ISocketMessage;

        ISocketMessage ReadMessage(ISocketConnection connection, MemoryStream stream);
        void WriteMessage(Socket socket, ISocketMessage message);
    }
}