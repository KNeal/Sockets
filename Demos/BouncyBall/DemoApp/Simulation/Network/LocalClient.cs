using System;
using System.Collections.Generic;
using BouncingBalls.Messages;
using BouncyBall.Client;
using SocketServer;
using SocketServer.Utils;

namespace DemoApp.Simulation
{
    public class LocalClient : BouncyBallClient
    {
        private GraphicsView _graphicsView;

        private Dictionary<string, BallAvatar> _remoteAvatars = new Dictionary<string, BallAvatar>();

        public LocalClient(GraphicsView graphicsView, string host, int port)
            : base(host, port)
        {
            _graphicsView = graphicsView;
        }

        public override void OnCreateBallMessage(ISocketConnection arg1, CreateBallMessage message)
        {
            BallAvatar ball = new BallAvatar
            {
                Id = message.BallId,
                Color = System.Drawing.Color.FromName(message.Color),
                Radius = message.Radius,
                PosX = message.XPos,
                PosY = message.YPos
            };

            lock (_remoteAvatars)
            {
                _remoteAvatars[ball.Id] = ball;
            }

            Logger.Info("[NetworkedSimulation.LocalClient] OnCreateBallMessage = BallId={0}", message.BallId);

            _graphicsView.Invoke(new Action(() => _graphicsView.AddBall(ball)));
        }


        public override void OnDestroyBallMessage(ISocketConnection arg1, DestroyBallMessage message)
        {
            BallAvatar ball;
            lock (_remoteAvatars)
            {
                if (_remoteAvatars.TryGetValue(message.BallId, out ball))
                {
                    _remoteAvatars.Remove(message.BallId);
                    _graphicsView.Invoke(new Action(() => _graphicsView.RemoveBall(ball)));
                }
            }
        }

        public override void OnUpdateBallMessage(ISocketConnection arg1, UpdateBallMessage message)
        {
            lock (_remoteAvatars)
            {
                BallAvatar ball;
                if (_remoteAvatars.TryGetValue(message.BallId, out ball))
                {
                    ball.PosX = message.XPos;
                    ball.PosY = message.YPos;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", UserName, ConnectionState);
        }
    }
}