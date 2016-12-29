namespace Sockets
{
    public interface ISocketMessageHandler
    {
        void RegisterMessageType<T>(string messageId);
    }
}