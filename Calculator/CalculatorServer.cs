using System;
using Sockets;

namespace Calculator
{
    public class CalculatorServer : SocketServer
    {
        public CalculatorServer()
        {
            CalculatorUtils.RegisterMessages(this);
        }

        protected override void OnClientConnected(ISocketClient client)
        {
            // Do nothing
        }

        protected override void OnClientDisconnected(ISocketClient client)
        {
            // Do nothing
        }

        protected override void OnMessage(int clientId, ISocketMessage message)
        {
            if (message is AddMessage)
            {
                OnAdd(clientId, (AddMessage)message);
            }
            else if (message is MultiplyMessage)
            {
                OnMultiply(clientId, (MultiplyMessage)message);
            }
            else if (message is PingMessage)
            {
                // Ignore
            }
            else
            {
                OnError(clientId, string.Format("[CalculatorServer] Unknown message type: {0}", message.GetType().FullName));
            }
        }

        private void OnAdd(int clientId, AddMessage addMessage)
        {
            Console.WriteLine("[CalculatorServer] OnAdd: {0} + {1}", addMessage.Value1, addMessage.Value2);

            CalculationResultMessage result = new CalculationResultMessage
            {
                Value = addMessage.Value1 + addMessage.Value2
            };

            SendMessage(clientId, result);
        }

        private void OnMultiply(int clientId, MultiplyMessage addMessage)
        {
            Console.WriteLine("[CalculatorServer] OnMultiply: {0} + {1}", addMessage.Value1, addMessage.Value2);

            CalculationResultMessage result = new CalculationResultMessage
            {
                Value = addMessage.Value1 * addMessage.Value2
            };

            SendMessage(clientId, result);
        }

        private void OnError(int clientId, string error)
        {
            Console.WriteLine("[CalculatorServer] OnError: {0}", error);

            CalculationErrorMessage result = new CalculationErrorMessage
            {
                Message = error
            };

            SendMessage(clientId, result);
        }
    }
}