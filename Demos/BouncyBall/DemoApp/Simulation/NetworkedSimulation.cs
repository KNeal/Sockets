using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BouncingBalls.Messages;
using BouncyBall.Client;
using BouncyBall.Server;
using SocketServer;

namespace DemoApp.Simulation
{
    public class NetworkedSimulation : ISimulation
    {
        private string _host = Dns.GetHostName();
        private int _port = 5000;

        private BouncyBallServer _server;
        private LocalClient _localClient;
        private List<RemoteClient> _remoteClients = new List<RemoteClient>();

        private DateTime _lastUpdateTime = DateTime.Now;
        
        public void Start(GraphicsView graphicsView)
        {
            _server = new BouncyBallServer();
            _server.Start(_port);

            _localClient = new LocalClient(graphicsView, _host, _port);
            _localClient.Connect("LocalClient", "password");
            
            IRoom room = new ControlRoom(graphicsView);

            Color[] colors = new[] {Color.Blue, Color.Teal, Color.BlueViolet, Color.Yellow, Color.Red, Color.Orange};
            foreach (var color in colors)
            {
                RemoteClient client = new RemoteClient(room, color, _host, _port);
                client.Connect("RemoteClient:" + color.Name, "password");
                _remoteClients.Add(client);
            }
        }

        public void Stop()
        {
            _remoteClients.ForEach(c => c.Dispose());
            _remoteClients.Clear();

            _localClient.Dispose();
            _localClient = null;

            _server.Dispose();
            _server = null;

        }

        public void Update()
        {
            TimeSpan deltaTime = DateTime.Now - _lastUpdateTime;

            // Update positions every 100 ms
            if (deltaTime.TotalMilliseconds > 100)
            {
                _lastUpdateTime = DateTime.Now;
                _remoteClients.ForEach(c => c.Update(deltaTime));
            }
        }

        private class LocalClient : BouncyBallClient
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

                Console.WriteLine("[NetworkedSimulation.LocalClient] OnCreateBallMessage = BallId={0}", message.BallId);

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

        private class RemoteClient : BouncyBallClient
        {
            private IRoom _room;
            private Color _color;
            private List<BallAvatar>  _balls = new List<BallAvatar>();
            private DateTime _lastUpdateTime;

            public RemoteClient(IRoom room, Color color, string host, int port)
                : base(host, port)
            {
                _room = room;
                _color = color;
            }

            protected override void OnConnected()
            {

                Console.WriteLine("[NetworkedSimulation.RemoteClient] OnConnected - {0}", UserName);
                Random rand = new Random();

                lock (_balls)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        BallAvatar ball = new BallAvatar()
                        {
                            Id = (_balls.Count + 1).ToString(),
                            Color = _color,
                            Radius = 30,
                            Room = _room,
                        };

                        ball.PosX = rand.Next(ball.Radius, _room.Width - ball.Radius);
                        ball.PosX = rand.Next(ball.Radius, _room.Height - ball.Radius);
                        ball.VelocityX = rand.Next(50, 100);
                        ball.VelocityY = rand.Next(50, 100);

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
}
