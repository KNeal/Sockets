using System;
using System.Collections.Generic;
using System.Drawing;

namespace DemoApp.Simulation
{
    public class LocalSimulation : ISimulation
    {
        List<BallAvatar> _balls = new List<BallAvatar>();
        private DateTime _lastUpdateTime = DateTime.Now;

        public void Start(GraphicsView graphicsView)
        {
            ControlRoom room = new ControlRoom(graphicsView);

            int ballId = 1;
            _balls.Add(new BallAvatar()
            {
                Id = (ballId++).ToString(),
                Color = Color.Red,
                Radius = 20,
                Room = room
            });
            _balls.Add(new BallAvatar()
            {
                Id = (ballId++).ToString(),
                Color = Color.Blue,
                Radius = 30,
                Room = room
            });
            _balls.Add(new BallAvatar()
            {
                Id = (ballId++).ToString(),
                Color = Color.ForestGreen,
                Radius = 20,
                Room = room
            });
            _balls.Add(new BallAvatar()
            {
                Id = (ballId++).ToString(),
                Color = Color.Yellow,
                Radius = 20,
                Room = room
            });
            _balls.Add(new BallAvatar()
            {
                Id = (ballId++).ToString(),
                Color = Color.Teal,
                Radius = 30,
                Room = room
            });

            
            Random rand = new Random();

            foreach (var ball in _balls)
            {
                ball.PosX = rand.Next(ball.Radius, room.Width - ball.Radius);
                ball.PosX = rand.Next(ball.Radius, room.Height - ball.Radius);
                ball.VelocityX = rand.Next(50, 100);
                ball.VelocityY = rand.Next(50, 100);
                graphicsView.AddBall(ball);
            }

            _lastUpdateTime = DateTime.Now;
        }

        public void Stop()
        {
           
        }

        public void Update()
        {
            TimeSpan deltaTime = DateTime.Now - _lastUpdateTime;
            _lastUpdateTime = DateTime.Now;
            foreach (var ball in _balls)
            {
                ball.Update(deltaTime.TotalSeconds);
            }

        }
    }
}