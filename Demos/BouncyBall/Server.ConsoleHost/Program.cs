using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BouncyBall.Server.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 500;

            Console.WriteLine("===============================================");
            Console.WriteLine("= BOUNCY BALL SERVER                          =");
            Console.WriteLine("===============================================");

            BouncyBallServer server = new BouncyBallServer();
            server.Start(Port);

            while (true)
            {
                // Spin
            }
        }
    }
}
