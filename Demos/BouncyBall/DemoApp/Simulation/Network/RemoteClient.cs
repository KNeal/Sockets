using System;
using System.Collections.Generic;
using System.Drawing;
using BouncyBall.Client;
using SocketServer.Utils;

namespace DemoApp.Simulation
{
    public class RemoteClient : BouncyBallClient
    {
        private IRoom _room;
        private Color _color;
        private int _ballCount;
        private List<BallAvatar> _balls = new List<BallAvatar>();
        private DateTime _lastUpdateTime;

        public RemoteClient(IRoom room, Color color, int ballCount, string host, int port)
            : base(host, port)
        {
            _room = room;
            _color = color;
            _ballCount = ballCount;
        }

        protected override void OnConnected()
        {
            Logger.Info("[NetworkedSimulation.RemoteClient] OnConnected - {0}", UserName);
            
            lock (_balls)
            {
                for (int i = 0; i < _ballCount; ++i)
                {
                    BallAvatar ball = new BallAvatar()
                    {
                        Id = (_balls.Count + 1).ToString(),
                        Color = _color,
                        Radius = 30,
                        Room = _room,
                    };

                    ball.PosX = Utils.Random.Next(ball.Radius, _room.Width - ball.Radius);
                    ball.PosX = Utils.Random.Next(ball.Radius, _room.Height - ball.Radius);
                    ball.VelocityX = Utils.Random.Next(50, 100);
                    ball.VelocityY = Utils.Random.Next(50, 100);

                    _balls.Add(ball);

                    SendCreateBall(ball.Id, ball.Color.Name, ball.Radius, (int)ball.PosX, (int)ball.PosY);
                }
            }
        }

        public void Update(TimeSpan deltaTime)
        {
            lock (_balls)
            {
                foreach (BallAvatar ball in _balls)
                {
                    ball.Update(deltaTime.TotalSeconds);
                    SendUpdateBall(ball.Id, (int)ball.PosX, (int)ball.PosY);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", UserName, ConnectionState);
        }
    }
}