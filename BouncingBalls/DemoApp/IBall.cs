using System.Drawing;

namespace DemoApp
{
    public interface IBall
    {
        int Id { get;  }
        Color Color { get; }
        double PosX { get; }
        double PosY { get; }
        int Radius { get; }
    }
}