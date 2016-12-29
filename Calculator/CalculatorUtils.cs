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
}