using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AWS.GameLift;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //GameLiftAPI.TestCallback();
            GameLiftAPI.GetSdkVersion();
            //GameLiftAPI.InitSDK();

            GameLiftServer server = new GameLiftServer();
            server.Initialize();
        }
    }
}
