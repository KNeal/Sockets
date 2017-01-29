using System;
using SocketServer.Utils;

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
            Logger.Info("[GameLiftServer] OnGameSessionStart");
        }

        private void OnProcessTerminate()
        {
            Logger.Info("[GameLiftServer] OnProcessTerminate");
        }

        private bool OnHealth()
        {
            Logger.Info("[GameLiftServer] OnHealth");
            return true;
        }
    }
}