using System;

namespace SocketServer.GameLift
{
    public class GameLiftServer
    {
        public void Initialize()
        {
            //GameLiftAPI.InitSDK();

            GameLiftAPI.ProcessReady(OnGameSessionStart, OnProcessTerminate, OnHealth, 2000, @"C:\Temp\gameliftlog.txt");
        }

        private void OnGameSessionStart(GameSession gameSession)
        {
            Console.WriteLine("[GameLiftServer] OnGameSessionStart");
        }

        private void OnProcessTerminate()
        {
            Console.WriteLine("[GameLiftServer] OnProcessTerminate");
        }

        private bool OnHealth()
        {
            Console.WriteLine("[GameLiftServer] OnHealth");
            return true;
        }
    }
}