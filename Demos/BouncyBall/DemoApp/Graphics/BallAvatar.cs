using System;
using System.Drawing;

namespace DemoApp
{
    public class BallAvatar : IBall
    {
        // IBall
        public string Id { get; set; }
        public Color Color { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public int Radius { get; set; }

        // Movement
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public IRoom Room { get; set; }

        public void Update(double timeElapsed)
        {
            // TODO.. actually calculate the collision and bounce angle

            PosX = PosX + VelocityX * timeElapsed;
            PosY = PosY + VelocityY * timeElapsed;

            if (PosX < 0)
            {
                PosX = 0;
                VelocityX *= -1;
            }

            if (PosX > Room.Width)
            {
                PosX = Room.Width;
                VelocityX *= -1;
            }

            if (PosY < 0)
            {
                PosY = 0;
                VelocityY *= -1;
            }

            if (PosY > Room.Height)
            {
                PosY = Room.Height;
                VelocityY *= -1;
            }

           // Console.WriteLine("[BallAvatar] {0}: X={1}, Y={2}", Id, PosX, PosY);
        }
    }
}