using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using BouncyBall.Server;

namespace DemoApp.Simulation
{
    public class NetworkedSimulation : ISimulation
    {
        private string _host = Dns.GetHostName();
        private int _port = 5000;

        private BouncyBallServer _server;
        private LocalClient _localClient;
        private List<RemoteClient> _remoteClients = new List<RemoteClient>();

        private DateTime _lastUpdateTime = DateTime.Now;
        
        public void Start(GraphicsView graphicsView)
        {
            _server = new BouncyBallServer();
            _server.Start(_port);

            _localClient = new LocalClient(graphicsView, _host, _port);
            _localClient.Connect("LocalClient", "password");
            
            IRoom room = new ControlRoom(graphicsView);

            Color[] colors = new[] {Color.Blue, Color.Teal, Color.BlueViolet, Color.Yellow, Color.Red, Color.Orange};
            foreach (var color in colors)
            {
                RemoteClient client = new RemoteClient(room, color, _host, _port);
                client.Connect("RemoteClient:" + color.Name, "password");
                _remoteClients.Add(client);
            }
        }

        public void Stop()
        {
            _remoteClients.ForEach(c => c.Dispose());
            _remoteClients.Clear();

            _localClient.Dispose();
            _localClient = null;

            _server.Dispose();
            _server = null;

        }

        public void Update()
        {
            TimeSpan deltaTime = DateTime.Now - _lastUpdateTime;

            // Update positions every 100 ms
            if (deltaTime.TotalMilliseconds > 100)
            {
                _lastUpdateTime = DateTime.Now;
                _remoteClients.ForEach(c => c.Update(deltaTime));
            }
        }

      
    }
}
