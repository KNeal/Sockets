using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DemoApp
{
    public partial class Form1 : Form
    {
        List<BallAvatar> _balls = new List<BallAvatar>();
        private DateTime _lastUpdateTime = DateTime.Now;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        } 

        public Form1()
        {
            InitializeComponent();
            GenerateBalls();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            UpdateBalls();
        }

        private void GenerateBalls()
        {
            this.DoubleBuffered = true;

            ControlRoom room = new ControlRoom(graphicsView);

            int ballId = 1;
            _balls.Add(new BallAvatar()
            {
                Id = ballId++,
                Color = Color.Red,
                Radius = 20,
                Room = room
            });
            _balls.Add(new BallAvatar()
            {
                Id = ballId++,
                Color = Color.Blue,
                Radius = 30,
                Room = room
            });
            _balls.Add(new BallAvatar()
            {
                Id = ballId++,
                Color = Color.ForestGreen,
                Radius = 20,
                Room = room
            });
            _balls.Add(new BallAvatar()
            {
                Id = ballId++,
                Color = Color.Yellow,
                Radius = 20,
                Room = room
            });
            _balls.Add(new BallAvatar()
            {
                Id = ballId++,
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

        private void UpdateBalls()
        {
            TimeSpan deltaTime = DateTime.Now - _lastUpdateTime;
            _lastUpdateTime = DateTime.Now;
            foreach (var ball in _balls)
            {
                ball.Update(deltaTime.TotalSeconds);
            }

            graphicsView.Refresh();
        }
    }
}
