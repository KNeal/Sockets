using System;
using Sockets;
using Sockets.Messages;

namespace Calculator
{
    public class CalculatorServer : SocketServer
    {
        public CalculatorServer()
        {
            RegisterMessageType<AddMessage>("AddMessage", OnAdd);
            RegisterMessageType<MultiplyMessage>("MultiplyMessage", OnMultiply);
            RegisterMessageType<PingRequestMessage>("PingRequestMessage", OnPingRequest);
        }

        protected override AuthResult AuthenticateClient(string userName, string userToken)
        {
            return AuthResult.Pass();
        }

        protected override void OnClientConnected(ISocketClient client)
        {
            // Do nothing
        }

        protected override void OnClientDisconnected(ISocketClient client)
        {
            // Do nothing
        }
        
        protected override void OnMessage(ISocketClient client, ISocketMessage message)
        {
          
        }

        private void OnAdd(ISocketClient client, AddMessage addMessage)
        {
            Console.WriteLine("[CalculatorServer] OnAdd: {0} + {1}", addMessage.Value1, addMessage.Value2);

            CalculationResultMessage result = new CalculationResultMessage
            {
                Value = addMessage.Value1 + addMessage.Value2
            };

            SendMessage(client.ConnectionId, result);
        }

        private void OnMultiply(ISocketClient client, MultiplyMessage addMessage)
        {
            Console.WriteLine("[CalculatorServer] OnMultiply: {0} + {1}", addMessage.Value1, addMessage.Value2);

            CalculationResultMessage result = new CalculationResultMessage
            {
                Value = addMessage.Value1 * addMessage.Value2
            };

            SendMessage(client.ConnectionId, result);
        }

        private void OnError(ISocketClient client, string error)
        {
            Console.WriteLine("[CalculatorServer] OnError: {0}", error);

            CalculationErrorMessage result = new CalculationErrorMessage
            {
                Message = error
            };

            SendMessage(client.ConnectionId, result);
        }
    }
}