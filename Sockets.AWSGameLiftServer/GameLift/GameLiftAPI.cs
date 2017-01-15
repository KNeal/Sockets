using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace AWS.GameLift
{
    public static class GameLiftAPI
    {
        public delegate void OnGameSessionStart(GameSession gameSession);
        public delegate void OnProcessTerminate();
        public delegate bool OnHealthCheck();

        #region NativeAPI
        private static class NativeAPI
        {
         
            private const string DLL_NAME = "aws-cpp-sdk-gamelift-server";

            
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void TestCallback(string result);

            [DllImport(DLL_NAME)]
            public static extern void test_callback(TestCallback callback);

            // Callbacks
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GameSessionStartCallback(IntPtr pGameSession);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ProcessTerminateCallback();
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool HealthCheckCallback();

            // API
            [DllImport(DLL_NAME)]
            public static extern IntPtr aws_gamelift_server__API_GetSdkVersion();
            [DllImport(DLL_NAME)]
            public static extern IntPtr aws_gamelift_server__API_InitSDK();
            [DllImport(DLL_NAME)]
            public static extern IntPtr aws_gamelift_server__API_ProcessReady(GameSessionStartCallback onGameStart,
                                                                             ProcessTerminateCallback onProcessTerminate,
                                                                             HealthCheckCallback onHealthCheck,
                                                                             int port, string logPath);

            // AwsGenericOutcome
            [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__AwsGenericOutcome_Delete(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern bool aws_gamelift__AwsGenericOutcome_IsSuccess(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern GameLiftErrorType aws_gamelift__AwsGenericOutcome_GetErrorType(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern GameLiftErrorType aws_gamelift__AwsGenericOutcome_GetErrorMessage(IntPtr pOutcome, int bufferLen, StringBuilder buffer);


            // AwsStringOutcome
            [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__AwsStringOutcome_Delete(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern bool aws_gamelift__AwsStringOutcome_IsSuccess(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern GameLiftErrorType aws_gamelift__AwsStringOutcome_GetErrorType(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern GameLiftErrorType aws_gamelift__AwsStringOutcome_GetErrorMessage(IntPtr pOutcome, int bufferLen, StringBuilder buffer);
            [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__AwsStringOutcome_GetString(IntPtr pOutcome, int bufferLen, StringBuilder buffer);

            // AwsStringOutcome
            [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__InitSDKOutcome_Delete(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern bool aws_gamelift__InitSDKOutcome_IsSuccess(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern GameLiftErrorType aws_gamelift__InitSDKOutcome_GetErrorType(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern GameLiftErrorType aws_gamelift__InitSDKOutcome_GetErrorMessage(IntPtr pOutcome, int bufferLen, StringBuilder buffer);
            [DllImport(DLL_NAME)]
            public static extern IntPtr aws_gamelift__InitSDKOutcome_GetServerState(IntPtr pOutcome);

            //// GameSession
            [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__InitSDKOutcome_GetGameSessionId(IntPtr pGameSession,  int bufferLen, StringBuilder buffer);
	        [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__InitSDKOutcome_GetName(IntPtr pGameSession, int bufferLen, StringBuilder buffer);
	        [DllImport(DLL_NAME)]
            public static extern void  aws_gamelift__InitSDKOutcome_GetFleetId(IntPtr pGameSession, int bufferLen, StringBuilder buffer);
	        [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__InitSDKOutcome_GetIpAddress(IntPtr pGameSession, int bufferLen, StringBuilder buffer);
	        [DllImport(DLL_NAME)]
            public static extern int  aws_gamelift__InitSDKOutcome_GetPort(IntPtr pGameSession);
	        [DllImport(DLL_NAME)]
            public static extern int  aws_gamelift__InitSDKOutcome_GetMaximumPlayerSessionCount(IntPtr pGameSession);
	        [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__InitSDKOutcome_GetGamePropertiesString(IntPtr pGameSession, int bufferLen, StringBuilder buffer, string pKeyValueDelim, string pPairDelim);

        }
        #endregion

        public static void TestCallback()
        {
            NativeAPI.test_callback(OnCallback);
        }

        private static void OnCallback(string result)
        {
            int xx = 0;
        }

        public static string GetSdkVersion()
        {
            string version = null;

            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_GetSdkVersion();
            if (pOutcome != IntPtr.Zero)
            {
                StringBuilder sb = new StringBuilder(1024);

                bool success = NativeAPI.aws_gamelift__AwsStringOutcome_IsSuccess(pOutcome);
                if (success)
                {
                    NativeAPI.aws_gamelift__AwsStringOutcome_GetString(pOutcome, sb.Capacity, sb);
                    version = sb.ToString();
                }
                else
                {
                    GameLiftErrorType error = NativeAPI.aws_gamelift__AwsStringOutcome_GetErrorType(pOutcome);
                    NativeAPI.aws_gamelift__AwsStringOutcome_GetErrorMessage(pOutcome, sb.Capacity, sb);

                    throw new Exception(string.Format("GameLiftError: {0}, Message: {1}", error, sb.ToString()));
                }

                NativeAPI.aws_gamelift__AwsStringOutcome_Delete(pOutcome);
            }

            return version;
        }

        public static bool InitSDK()
        {
            IntPtr serverState = IntPtr.Zero;

            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_InitSDK();
            if (pOutcome != IntPtr.Zero)
            {
                bool success = NativeAPI.aws_gamelift__InitSDKOutcome_IsSuccess(pOutcome);
                if (success)
                {
                    serverState = NativeAPI.aws_gamelift__InitSDKOutcome_GetServerState(pOutcome);
                }
                else
                {
                    GameLiftErrorType error = NativeAPI.aws_gamelift__InitSDKOutcome_GetErrorType(pOutcome);
                    StringBuilder sb = new StringBuilder(1024);
                    NativeAPI.aws_gamelift__InitSDKOutcome_GetErrorMessage(pOutcome, sb.Capacity, sb);
                    string errorMessage = sb.ToString();

                    throw new Exception(string.Format("GameLiftError: {0}, Message: {1}", error, errorMessage));
                }

                NativeAPI.aws_gamelift__InitSDKOutcome_Delete(pOutcome);
            }

            return serverState != IntPtr.Zero;
        }

        public static bool ProcessReady(OnGameSessionStart onGameSessionStart, OnProcessTerminate onProcessTerminate, OnHealthCheck onHealthCheck, int port, string logDirectory)
        {
            // Wrap the Native API Call backs

            NativeAPI.GameSessionStartCallback handleSessionStartCallback = (IntPtr pGameSession) =>
            {
                try
                {Console.WriteLine("onGameSessionStart called");


                    // (1) Parse the Data
                    StringBuilder sb = new StringBuilder(4096);
                    GameSession gameSession = new GameSession();

                    // GameSessionId
                    NativeAPI.aws_gamelift__InitSDKOutcome_GetGameSessionId(pGameSession, sb.Capacity, sb);
                    gameSession.GameSessionId = sb.ToString();

                    // Name
                    NativeAPI.aws_gamelift__InitSDKOutcome_GetName(pGameSession, sb.Capacity, sb);
                    gameSession.Name = sb.ToString();

                    // IpAddress
                    NativeAPI.aws_gamelift__InitSDKOutcome_GetIpAddress(pGameSession, sb.Capacity, sb);
                    gameSession.IpAddress = sb.ToString();

                    // Port
                    gameSession.Port = NativeAPI.aws_gamelift__InitSDKOutcome_GetPort(pGameSession);

                    // FleetId
                    NativeAPI.aws_gamelift__InitSDKOutcome_GetFleetId(pGameSession, sb.Capacity, sb);
                    gameSession.FleetId = sb.ToString();

                    // MaxPlayerSessionCounts
                    gameSession.MaxPlayerSessionCounts = NativeAPI.aws_gamelift__InitSDKOutcome_GetMaximumPlayerSessionCount(pGameSession);
                    
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    const string keyValueDelim = "|=|";
                    const string pairDelim = "|;|";
                    NativeAPI.aws_gamelift__InitSDKOutcome_GetGamePropertiesString(pGameSession, sb.Capacity, sb, keyValueDelim, pairDelim);
                    string[] pairs = sb.ToString().Split(new [] {pairDelim }, StringSplitOptions.None);
                    foreach (string pair in pairs)
                    {
                        string[] tokens = pair.Split(new [] {keyValueDelim }, StringSplitOptions.None);
                        if (tokens.Length == 2)
                        {
                            dict[tokens[0]] = dict[tokens[1]];
                        }
                    }
                    gameSession.GameProperties = dict;

                    // (2) Trigger the callback
                    onGameSessionStart(gameSession);
                }
                catch (Exception e)
                {
                    Console.WriteLine("onGameSessionStart failed: {0}", e);
                }
            };

            NativeAPI.ProcessTerminateCallback handleProcessTerminateCallback = () =>
            {
                try
                {
                    Console.WriteLine("onProcessTerminate called");
                    onProcessTerminate();
                }
                catch (Exception e)
                {
                    Console.WriteLine("onProcessTerminate failed: {0}", e);
                }
            };

            NativeAPI.HealthCheckCallback handleHealthCallback = () =>
            {
                try
                {
                    Console.WriteLine("onHealthCheck called");
                    return onHealthCheck();
                }
                catch (Exception e)
                {
                    Console.WriteLine("onHealthCheck failed: {0}", e);
                    return false;
                }
            };


            // Make the Native Call

            bool success = false;

            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_ProcessReady(handleSessionStartCallback,
                handleProcessTerminateCallback, handleHealthCallback, port, logDirectory);

            if (pOutcome != IntPtr.Zero)
            {
                success = NativeAPI.aws_gamelift__AwsGenericOutcome_IsSuccess(pOutcome);
                if (!success)
                {
                    GameLiftErrorType error = NativeAPI.aws_gamelift__AwsGenericOutcome_GetErrorType(pOutcome);
                    StringBuilder sb = new StringBuilder(1024);
                    NativeAPI.aws_gamelift__AwsGenericOutcome_GetErrorMessage(pOutcome, sb.Capacity, sb);
                    string errorMessage = sb.ToString();

                    Console.WriteLine("GameLiftError: {0}, Message: {1}", error, errorMessage);
                }

                NativeAPI.aws_gamelift__AwsStringOutcome_Delete(pOutcome);
            }

            return success;
        }
    }
}
