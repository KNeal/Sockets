using System;
using System.Threading;
using SocketServer;
using SocketServer.GameLift;
using SocketServer.Utils;

namespace BouncyBall.Server.GameLiftHost
{
    class Program
    {
        private const int Port = 500;
        private const string LogFile = "Bouncyball.log";

        private static bool _running = true;
        
        static void Main(string[] args)
        {
            try
            {
                Logger.Initialize(LogFile);

                Logger.Info("===============================================");
                Logger.Info("= BOUNCY BALL SERVER                          =");
                Logger.Info("===============================================");

                Logger.Info("[GameLiftHost] Calling GameLiftAPI.InitSdk");
                GameLiftAPI.InitSdk();

                Logger.Info("[GameLiftHost] Initializing BouncyBallGameLiftServer");
                BouncyBallGameLiftServer server = new BouncyBallGameLiftServer();
                server.Start(Port);

                Logger.Info("[GameLiftHost] Calling GameLiftAPI.ProcessReady");
                GameLiftAPI.ProcessReady(OnGameSessionStart, OnProcessTerminate, OnHealthCheck, Port, LogFile);
                
                while (_running)
                {
                    // TODO.. replace with signal.
                    Thread.Sleep(100);
                }
                
                Logger.Info("[GameLiftHost] Shutting down BouncyBallGameLiftServer");
                server.Dispose();

                Logger.Info("[GameLiftHost] Calling GameLiftAPI.Destroy");
                GameLiftAPI.Destroy();
            }
            catch (Exception e)
            {
                Logger.Error("[GameLiftHost] Error: {0}", e);
            }
        }

        public static void OnGameSessionStart(GameSession gameSession)
        {
            Logger.Info("[GameLiftHost] OnGameSessionStart: {0}", gameSession);
            GameLiftAPI.ActivateGameSession();
        }

        public static void OnProcessTerminate()
        {
            Logger.Info("[GameLiftHost] OnProcessTerminate");
            _running = false;
        }

        public static bool OnHealthCheck()
        {
            Logger.Info("[GameLiftHost] OnHealthCheck");
            return true;
        }

        private class BouncyBallGameLiftServer : BouncyBallServer
        {
            protected override void OnClientConnected(ISocketConnection client)
            {
                GameLiftAPI.AcceptPlayerSession(client.ConnectionName);
                base.OnClientConnected(client);
            }

            protected override void OnClientDisconnected(ISocketConnection client)
            {
                base.OnClientDisconnected(client);
                GameLiftAPI.RemovePlayerSession(client.ConnectionName);
            }
        }
    }
}
