using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Calculator;

namespace CalculatorClientApp
{
    class Program
    {
        private static void InteractiveClient()
        {
            CalculatorClient client = new CalculatorClient(Dns.GetHostName(), 5000);
            client.Start();

            while (true)
            {
                Console.WriteLine("Enter command: ");
                string line = Console.ReadLine();

                try
                {
                    string[] fields = line.Split(' ');
                    if (fields.Length == 3)
                    {
                        string command = fields[0].ToLower();
                        int value1 = Int32.Parse(fields[1]);
                        int value2 = Int32.Parse(fields[2]);

                        switch (command)
                        {
                            case "add":
                                client.Add(value1, value2);
                                break;

                            case "multiply":
                                client.Add(value1, value2);
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("invalid command");
                }
            }
        }

        private static void AutomatedTest()
        {
            CalculatorServer server = new CalculatorServer();
            server.Start(5000);

            CalculatorClient client = new CalculatorClient(Dns.GetHostName(), 5000);
            client.Start();

            Thread.Sleep(1000);

            client.Add(1, 3);
            Thread.Sleep(2000);

            client.Multiply(5, 2);
            Thread.Sleep(2000);

            Console.ReadLine();
        }

        private static void Main(string[] args)
        {
            //InteractiveClient();

            AutomatedTest();
        }
    }
}
