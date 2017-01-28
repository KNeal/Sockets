using System.Drawing;

namespace DemoApp
{
    public interface IBall
    {
        string Id { get;  }
        Color Color { get; }
        double PosX { get; }
        double PosY { get; }
        int Radius { get; }
    }
}