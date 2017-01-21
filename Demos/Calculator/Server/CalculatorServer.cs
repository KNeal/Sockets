using System;
using Calculator.Messages;
using SocketServer;
using SocketServer.Messages;

namespace Calculator
{
    public class CalculatorServer : SocketServer.Server.SocketServer
    {
        public CalculatorServer()
        {
            RegisterMessageType<AddMessage>("AddMessage", OnAdd);
            RegisterMessageType<MultiplyMessage>("MultiplyMessage", OnMultiply);
        }

        private void OnAuthRequestMessage(ISocketConnection arg1, AuthRequestMessage arg2)
        {
            throw new NotImplementedException();
        }

        protected override AuthResult AuthenticateClient(string userName, string userToken)
        {
            return AuthResult.Pass();
        }

        protected override void OnClientConnected(ISocketConnection client)
        {
            // Do nothing
        }

        protected override void OnClientDisconnected(ISocketConnection client)
        {
            // Do nothing
        }

        protected override void OnMessage(ISocketConnection client, ISocketMessage message)
        {
          
        }

        private void OnAdd(ISocketConnection client, AddMessage addMessage)
        {
            Console.WriteLine("[CalculatorServer] OnAdd: {0} + {1}", addMessage.Value1, addMessage.Value2);

            CalculationResultMessage result = new CalculationResultMessage
            {
                Value = addMessage.Value1 + addMessage.Value2
            };

            SendMessage(client.ConnectionId, result);
        }

        private void OnMultiply(ISocketConnection client, MultiplyMessage addMessage)
        {
            Console.WriteLine("[CalculatorServer] OnMultiply: {0} + {1}", addMessage.Value1, addMessage.Value2);

            CalculationResultMessage result = new CalculationResultMessage
            {
                Value = addMessage.Value1 * addMessage.Value2
            };

            SendMessage(client.ConnectionId, result);
        }

        private void OnError(ISocketConnection client, string error)
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