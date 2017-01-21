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
            RegisterMessageType<CreateBallMessage>("CreateBallMessage", OnSpawnBallMessage);
            RegisterMessageType<UpdateBallMessage>("UpdateBallPositionMessage", OnUpdateBallPositionMessage);
        }

        #region Connection Management
        protected override AuthResult AuthenticateClient(string userName, string userToken)
        {
            // Always Authenticate
            return AuthResult.Pass();
        }

        protected override void OnClientConnected(ISocketConnection client)
        {
            // Send the sate of the world.
            _world.InitializeClient(client, this);
        }

        protected override void OnClientDisconnected(ISocketConnection client)
        {
            // Do nothing
            _world.RemoveClient(client, this);
        }

        #endregion

        #region Messages

        private void OnUpdateBallPositionMessage(ISocketConnection client, UpdateBallMessage message)
        {
            _world.UpdateBall(client, this, message);
        }

        private void OnSpawnBallMessage(ISocketConnection client, CreateBallMessage message)
        {
            _world.CreateBall(client, this, message);
        }


        #endregion
    }
}