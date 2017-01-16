namespace Sockets
{
    public interface IClientCredentials
    {
        string Name { get; set; }
        string Token { get; set; }
    }
}