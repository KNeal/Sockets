using System;
using System.IO;
using BouncingBalls.Messages;
using SocketServer;

namespace BouncyBall.Client
{
    public class BouncyBallClient : SocketServerClient
    {
        public BouncyBallClient(string host, int port) 
            : base(host, port)
        {
            RegisterMessageType<CreateBallMessage>("CreateBallMessage", OnCreateBallMessage);
            RegisterMessageType<DestroyBallMessage>("DestroyBallMessage", OnDestroyBallMessage);
            RegisterMessageType<UpdateBallMessage>("UpdateBallMessage", OnUpdateBallMessage);
        }

        public virtual void OnUpdateBallMessage(ISocketConnection arg1, UpdateBallMessage message)
        {
            
        }

        public virtual void OnDestroyBallMessage(ISocketConnection arg1, DestroyBallMessage message)
        {
            
        }

        public virtual void OnCreateBallMessage(ISocketConnection arg1, CreateBallMessage message)
        {
            
        }

        public void SendCreateBall(string ballId, string color, int radius, int posX, int posY)
        {
            CreateBallMessage message = new CreateBallMessage
            {
                BallId = ballId,
                Radius = radius,
                Color = color,
                XPos = posX,
                YPos = posY
            };

            SendMessage(message);
        }

        public void SendUpdateBall(string ballId, int posX, int posY)
        {
            UpdateBallMessage message = new UpdateBallMessage
            {
                BallId = ballId,
                XPos = posX,
                YPos = posY
            };

            SendMessage(message);
        }
    }
}
