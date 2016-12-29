using System;
using Sockets;

namespace Calculator
{
    public class CalculatorClient : SocketClient
    {
        public CalculatorClient(string host, int port)
            :base(host, port)
        {
            CalculatorUtils.RegisterMessages(this);
        }

        protected override void OnMessage(ISocketMessage message)
        {
            if (message is CalculationResultMessage)
            {
                OnCalculationResultMessage((CalculationResultMessage)message);
            }
            else if (message is CalculationErrorMessage)
            {
                OnCalculationErrorMessage((CalculationErrorMessage)message);
            }
            else if (message is PongMessage)
            {
                OnPongMessage((PongMessage)message);
            }
        }

        private void OnCalculationResultMessage(CalculationResultMessage message)
        {
            Console.WriteLine("[CalculatorClient] Result: {0}", message.Value);
        }

        private void OnCalculationErrorMessage(CalculationErrorMessage message)
        {
            Console.WriteLine("[CalculatorClient] Error: {0}", message.Message);
        }


        private void OnPongMessage(PongMessage message)
        {
            Console.WriteLine("[CalculatorClient] Ping Time: {0}ms", message.ElapsedTime.TotalMilliseconds);
        }

        public void Add(int value1, int value2)
        {
            AddMessage message = new AddMessage()
            {
                Value1 = value1,
                Value2 = value2
            };

            SendMessage(message);
        }
        
        public void Multiply(int value1, int value2)
        {
            MultiplyMessage message = new MultiplyMessage()
            {
                Value1 = value1,
                Value2 = value2
            };

            SendMessage(message);
        }
    }
}