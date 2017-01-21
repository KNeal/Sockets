using SocketServer;

namespace BouncyBall.Server.State
{
    public class BallState
    {
        public ISocketConnection Client { get; set; }
        public string BallId { get; set; }
        public string Color { get; set; }
        public int Radius { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }

    }
}
