using System;
using System.Collections.Generic;
using SocketServer;
using SocketServer.Messages;

namespace Calculator
{
    public class CalculatorClient : SocketClient
    {
        public List<int> ResultHistory = new List<int>();

        public CalculatorClient(string host, int port)
            : base(host, port)
        {
            RegisterMessageType<CalculationResultMessage>("CalculationResultMessage", OnCalculationResultMessage);
            RegisterMessageType<CalculationErrorMessage>("CalculationErrorMessage", OnCalculationErrorMessage);
            RegisterMessageType<PingResponseMessage>("PingResponseMessage", OnPingResponseMessage);
        }

        private void OnCalculationResultMessage(ISocketConnection connection, CalculationResultMessage message)
        {
            Console.WriteLine("[CalculatorClient] Result: {0}", message.Value);

            lock (ResultHistory)
            {
                ResultHistory.Add(message.Value);
            }
        }

        private void OnCalculationErrorMessage(ISocketConnection connection, CalculationErrorMessage message)
        {
            Console.WriteLine("[CalculatorClient] Error: {0}", message.Message);
        }

        private void OnPingResponseMessage(ISocketConnection connection, PingResponseMessage message)
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

        protected override void OnMessage(ISocketMessage message)
        {
            throw new NotImplementedException();
        }
    }
}