using System;
using System.Net;
using System.Threading;
using Calculator;
using NUnit.Framework;
using SocketServer;

namespace CalculatorTests
{
    [TestFixture]
    public class CalculatorTests
    {
        [Test]
        public void Test()
        {
            const int port = 5000;
            using (CalculatorServer server = new CalculatorServer())
            {
                server.Start(port);

                using (CalculatorClient client = new CalculatorClient(Dns.GetHostName(), port))
                {
                    client.Connect("TestUser", "TestPassword");

                    while (client.ConnectionState != SocketServerClient.State.Connected)
                    {
                        //Console.WriteLine("Waiting for client connection");
                    }

                    Thread.Sleep(1000);

                    client.Add(1, 3);
                    Thread.Sleep(2000);
                    Assert.AreEqual(4, client.ResultHistory[0]);

                    client.Multiply(5, 2);
                    Thread.Sleep(2000);
                    Assert.AreEqual(10, client.ResultHistory[1]);

                    Thread.Sleep(2000);
                }
            }
          
        }
    }
}
