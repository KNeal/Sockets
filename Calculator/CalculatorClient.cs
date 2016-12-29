using System;
using Sockets;

namespace Calculator
{

    public class CalculatorUtils
    {
        public static void RegisterMessages(SocketMessageHandler messageHandler)
        {
            messageHandler.RegisterMessageType<AddMessage>("AddMessage");
            messageHandler.RegisterMessageType<MultiplyMessage>("MultiplyMessage");
            messageHandler.RegisterMessageType<CalculationResultMessage>("CalculationResultMessage");
            messageHandler.RegisterMessageType<CalculationErrorMessage>("CalculationErrorMessage");
        }
    }

    public class CalculatorClient : SocketClient
    {
        public CalculatorClient()
        {
            CalculatorUtils.RegisterMessages(this);
        }

        protected override void OnMessage(ISocketMessage message)
        {
            if (message is CalculationResultMessage)
            {
                OnCalculationResultMessage((CalculationResultMessage)message);
            }
            else
            {
                if (message is CalculationErrorMessage)
                {
                    OnCalculationErrorMessage((CalculationErrorMessage)message);
                }
            }
        }

        private void OnCalculationResultMessage(CalculationResultMessage message)
        {
            Console.WriteLine("[CalculatorServer] Result: {0}", message.Value);
        }

        private void OnCalculationErrorMessage(CalculationErrorMessage message)
        {
            Console.WriteLine("[CalculatorServer] Error: {0}", message.Message);
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
    }
}