using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Calculator;
using SocketServer;

namespace CalculatorClientApp
{
    class Program
    {
        private static void InteractiveClient()
        {
            CalculatorClient client = new CalculatorClient(Dns.GetHostName(), 5000);
            client.Connect("TestUser", "TestPassword");

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
        
        private static void Main(string[] args)
        {
            InteractiveClient();
        }
    }
}
