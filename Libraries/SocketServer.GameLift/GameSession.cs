using System.Collections.Generic;

namespace SocketServer.GameLift
{
    public class GameSession
    {
        public string GameSessionId { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string FleetId { get; set; }
        public int Port { get; set; }
        public int MaxPlayerSessionCounts { get; set; }
        public Dictionary<string, string> GameProperties { get; set; }

        public override string ToString()
        {
            return string.Format("GameSessionId: {0}, Name: {1}, IpAddress: {2}, FleetId: {3}, Port: {4}, MaxPlayerSessionCounts: {5}, GameProperties: {6}", GameSessionId, Name, IpAddress, FleetId, Port, MaxPlayerSessionCounts, GameProperties);
        }
    }
}