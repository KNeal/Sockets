using System.Windows.Forms;

namespace DemoApp
{
    public class ControlRoom : IRoom
    {
        private Control _control;

        public int Width
        {
            get { return _control.Width; }
        }

        public int Height
        {
            get { return _control.Height; }
        }

        public ControlRoom(Control control)
        {
            _control = control;
        }
    }
}