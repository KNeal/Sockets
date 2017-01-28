namespace DemoApp.Simulation
{
    public interface ISimulation
    {
        void Start(GraphicsView graphicsView);
        void Stop();
        void Update();
    }
}