using SocketServer;

namespace BouncyBall.Client
{
    public class BouncyBallClient : SocketServerClient
    {
        public BouncyBallClient(string host, int port) 
            : base(host, port)
        {
        }


    }
}
