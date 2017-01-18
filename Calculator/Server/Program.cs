using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calculator;

namespace CalculatorServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            CalculatorServer server = new CalculatorServer();
            server.Start(5000);

            while (true)
            {
                
            }
        }
    }
}
