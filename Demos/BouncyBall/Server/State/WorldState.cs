using System.Collections.Generic;
using BouncingBalls.Messages;
using SocketServer;

namespace BouncyBall.Server.State
{
    public class WorldState
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, BallState> _balls = new Dictionary<string, BallState>();

        public void InitializeClient(ISocketConnection client, ISocketServer server)
        {
            lock (_lock)
            {
                // Send the initial state for each ball.
                foreach (var ball in _balls.Values)
                {
                    server.SendMessage(client.ConnectionId, new CreateBallMessage
                    {
                        BallId = ball.BallId,
                        Color = ball.Color,
                        Radius = ball.Radius,
                        XPos = ball.Radius,
                        YPos = ball.Radius
                    });
                }
            }
        }

        public void RemoveClient(ISocketConnection client, ISocketServer server)
        {
            lock (_lock)
            {
                // Remove each ball owned by this user.
                foreach (var ball in _balls.Values)
                {
                    if (ball.Client.ConnectionId == client.ConnectionId)
                    {
                        _balls.Remove(ball.BallId);
                        server.SendMessageToAllClients(new DestroyBallMessage
                        {
                            BallId = ball.BallId,
                        });
                    }
                }
            }
        }

        public void CreateBall(ISocketConnection client, 
            ISocketServer server,
            CreateBallMessage posMessage)
        {
            lock (_lock)
            {
                BallState ball;
                if (_balls.TryGetValue(posMessage.BallId, out ball))
                {
                    ball.PosX = posMessage.XPos;
                    ball.PosY = posMessage.YPos;
                    
                    server.SendMessageToAllClients(posMessage, client.ConnectionId);
                }
            }
        }

        public void UpdateBall(ISocketConnection client,
            ISocketServer server,
            UpdateBallMessage posMessage)
        {
            lock (_lock)
            {
                BallState ball;
                if (_balls.TryGetValue(posMessage.BallId, out ball))
                {
                    ball.PosX = posMessage.XPos;
                    ball.PosY = posMessage.YPos;

                    server.SendMessageToAllClients(posMessage, client.ConnectionId);
                }
            }
        }
    }
}