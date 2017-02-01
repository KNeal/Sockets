using System;
using BouncingBalls.Messages;
using BouncyBall.Server.State;
using SocketServer;

namespace BouncyBall.Server
{
    public class BouncyBallServer : SocketServer.Server.SocketServer
    {
        readonly WorldState _world = new WorldState();

        public BouncyBallServer()
        {
            RegisterMessageType<CreateBallMessage>(OnCreateBallMessage);
            RegisterMessageType<UpdateBallMessage>(OnUpdateBallPositionMessage);
        }

        #region Connection Management
        protected override AuthResult AuthenticateClient(string userName, string password)
        {
            //Console.WriteLine("[BouncyBallServer] AuthenticateClient userName={0}, password={1}", userName, password);

            // Always Authenticate
            return AuthResult.Pass();
        }

        protected override void OnClientConnected(ISocketConnection client)
        {
            // Send the state of the world to the client.
            _world.InitializeClient(client, this);
        }

        protected override void OnClientDisconnected(ISocketConnection client)
        {
            // Do nothing
            _world.RemoveClient(client, this);
        }

        #endregion

        #region Messages

        private void OnCreateBallMessage(ISocketConnection client, CreateBallMessage message)
        {
            _world.CreateBall(client, this, message);
        }

        private void OnUpdateBallPositionMessage(ISocketConnection client, UpdateBallMessage message)
        {
            _world.UpdateBall(client, this, message);
        }

        #endregion
    }
}