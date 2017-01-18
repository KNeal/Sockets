using System;
using System.Drawing;

namespace DemoApp
{
    public class BallAvatar : IBall
    {
        // IBall
        public int Id { get; set; }
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

            if (PosX < 0 || PosX > Room.Width)
            {
                VelocityX *= -1;
            }

            if (PosY < 0 || PosY > Room.Height)
            {
                VelocityY *= -1;
            }

           // Console.WriteLine("[BallAvatar] {0}: X={1}, Y={2}", Id, PosX, PosY);
        }
    }
}