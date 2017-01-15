using System.Collections.Generic;

namespace AWS.GameLift
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
    }
}