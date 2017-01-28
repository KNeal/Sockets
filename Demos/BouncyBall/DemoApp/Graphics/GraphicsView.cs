using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace DemoApp
{
    public partial class GraphicsView : UserControl
    {
        private Dictionary<string, IBall> _balls = new Dictionary<string, IBall>();

        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        public GraphicsView()
        {
            InitializeComponent();

        }

        private void GraphicsView_Load(object sender, EventArgs e)
        {
           // this.DoubleBuffered = true;
        }

        public void AddBall(IBall ball)
        {
            _balls[ball.Id] = ball;
        }

        public void RemoveBall(IBall ball)
        {
            if (_balls.ContainsKey(ball.Id))
            {
                _balls.Remove(ball.Id);
            }
        }

        private void GraphicsView_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = CreateGraphics();

            foreach (IBall ball in _balls.Values)
            {
                SolidBrush brush = new SolidBrush(ball.Color);

                Rectangle rect = new Rectangle((int) Math.Round(ball.PosX - ball.Radius),
                    (int) Math.Round(ball.PosY - ball.Radius),
                    ball.Radius,
                    ball.Radius);

                graphics.FillEllipse(brush, rect);

                brush.Dispose();
            }

            graphics.Dispose();
        }
    }
}
