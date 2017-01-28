﻿using System.Collections.Generic;
using System.Text;
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
            CreateBallMessage  message)
        {
            lock (_lock)
            {
                string ballId = string.Format("{0}-{1}", client.ConnectionId, message.BallId);
                BallState ball = new BallState
                {
                    BallId = ballId,
                    Client = client,
                    Color = message.Color,
                    Radius = message.Radius,
                    PosX = message.XPos,
                    PosY = message.YPos
                };

                _balls[ball.BallId] = ball;

                CreateBallMessage m = new CreateBallMessage
                {
                    BallId = ballId,
                    Color = ball.Color,
                    Radius = ball.Radius,
                    XPos = ball.PosX,
                    YPos = ball.PosY
                };
                
                server.SendMessageToAllClients(m, client.ConnectionId);
            }
        }

        public void UpdateBall(ISocketConnection client,
            ISocketServer server,
            UpdateBallMessage message)
        {
            lock (_lock)
            {
                string ballId = string.Format("{0}-{1}", client.ConnectionId, message.BallId);
                BallState ball;
                if (_balls.TryGetValue(ballId, out ball))
                {
                    ball.PosX = message.XPos;
                    ball.PosY = message.YPos;

                    UpdateBallMessage m = new UpdateBallMessage
                    {
                        BallId = ballId,
                        XPos = ball.PosX,
                        YPos = ball.PosY
                    };

                    server.SendMessageToAllClients(m, client.ConnectionId);
                }
            }
        }
    }
}