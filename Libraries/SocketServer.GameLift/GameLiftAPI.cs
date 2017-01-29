using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SocketServer.Utils;

namespace SocketServer.GameLift
{
    public static class GameLiftAPI
    {
        public delegate void OnGameSessionStart(GameSession gameSession);
        public delegate void OnProcessTerminate();
        public delegate bool OnHealthCheck();

        #region NativeAPI
        private static class NativeAPI
        {
            private const string GameLiftDLLName = "aws-cpp-sdk-gamelift-server";

            // Testing
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void TestCallback(string result);

            [DllImport(GameLiftDLLName)]
            public static extern void test_callback(TestCallback callback);

            // Callbacks
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void GameSessionStartCallback(IntPtr pGameSession);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ProcessTerminateCallback();
            
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool HealthCheckCallback();

            // API
            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_GetSdkVersion();
           
            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_InitSDK();

            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_ProcessReady(GameSessionStartCallback onGameStart,
                                                                            ProcessTerminateCallback onProcessTerminate,
                                                                            HealthCheckCallback onHealthCheck,
                                                                            int port,
                                                                            string logPath);


            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_ProcessEnding();

            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_ActivateGameSession();

            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_TerminateGameSession();

            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_GetGameSessionId();

            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_AcceptPlayerSession(string playerSession);

            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_RemovePlayerSession(string pPlayerSession);

            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift_server__API_Destroy();

            // AwsGenericOutcome
            [DllImport(GameLiftDLLName)]
            public static extern void aws_gamelift__AwsGenericOutcome_Delete(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern bool aws_gamelift__AwsGenericOutcome_IsSuccess(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern GameLiftErrorType aws_gamelift__AwsGenericOutcome_GetErrorType(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern GameLiftErrorType aws_gamelift__AwsGenericOutcome_GetErrorMessage(IntPtr pOutcome, int bufferLen, StringBuilder buffer);
            
            // AwsStringOutcome
            [DllImport(GameLiftDLLName)]
            public static extern void aws_gamelift__AwsStringOutcome_Delete(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern bool aws_gamelift__AwsStringOutcome_IsSuccess(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern GameLiftErrorType aws_gamelift__AwsStringOutcome_GetErrorType(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern GameLiftErrorType aws_gamelift__AwsStringOutcome_GetErrorMessage(IntPtr pOutcome, int bufferLen, StringBuilder buffer);
            [DllImport(GameLiftDLLName)]
            public static extern void aws_gamelift__AwsStringOutcome_GetString(IntPtr pOutcome, int bufferLen, StringBuilder buffer);

            // AwsStringOutcome
            [DllImport(GameLiftDLLName)]
            public static extern void aws_gamelift__InitSDKOutcome_Delete(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern bool aws_gamelift__InitSDKOutcome_IsSuccess(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern GameLiftErrorType aws_gamelift__InitSDKOutcome_GetErrorType(IntPtr pOutcome);
            [DllImport(GameLiftDLLName)]
            public static extern GameLiftErrorType aws_gamelift__InitSDKOutcome_GetErrorMessage(IntPtr pOutcome, int bufferLen, StringBuilder buffer);
            [DllImport(GameLiftDLLName)]
            public static extern IntPtr aws_gamelift__InitSDKOutcome_GetServerState(IntPtr pOutcome);

            //// GameSession
            [DllImport(GameLiftDLLName)]
            public static extern void aws_gamelift__InitSDKOutcome_GetGameSessionId(IntPtr pGameSession,  int bufferLen, StringBuilder buffer);
	        [DllImport(GameLiftDLLName)]
            public static extern void aws_gamelift__InitSDKOutcome_GetName(IntPtr pGameSession, int bufferLen, StringBuilder buffer);
	        [DllImport(GameLiftDLLName)]
            public static extern void aws_gamelift__InitSDKOutcome_GetFleetId(IntPtr pGameSession, int bufferLen, StringBuilder buffer);
	        [DllImport(GameLiftDLLName)]
            public static extern void aws_gamelift__InitSDKOutcome_GetIpAddress(IntPtr pGameSession, int bufferLen, StringBuilder buffer);
	        [DllImport(GameLiftDLLName)]
            public static extern int  aws_gamelift__InitSDKOutcome_GetPort(IntPtr pGameSession);
	        [DllImport(GameLiftDLLName)]
            public static extern int  aws_gamelift__InitSDKOutcome_GetMaximumPlayerSessionCount(IntPtr pGameSession);
	        [DllImport(GameLiftDLLName)]
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
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_GetSdkVersion();
            return ReadStringOutcome("GetSdkVersion", pOutcome);
        }

        public static bool InitSdk()
        {
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_InitSDK();
            return ReadInitSdkOutcome("InitSdk", pOutcome);
        }

        public static bool ProcessReady(OnGameSessionStart onGameSessionStart, 
            OnProcessTerminate onProcessTerminate, 
            OnHealthCheck onHealthCheck, 
            int port, 
            string logPath)
        {
            // Wrap the Native API Call backs

            NativeAPI.GameSessionStartCallback handleSessionStartCallback = (IntPtr pGameSession) =>
            {
                try
                {
                    Logger.Info("[GameLiftAPI] onGameSessionStart called");
                    GameSession gameSession = ReadGameSession("onGameSessionStart", pGameSession);
                    onGameSessionStart(gameSession);
                }
                catch (Exception e)
                {
                    Logger.Error("[GameLiftAPI] onGameSessionStart failed: {0}", e);
                }
            };

            NativeAPI.ProcessTerminateCallback handleProcessTerminateCallback = () =>
            {
                try
                {
                    Logger.Info("[GameLiftAPI] onProcessTerminate called");
                    onProcessTerminate();
                }
                catch (Exception e)
                {
                    Logger.Error("[GameLiftAPI] onProcessTerminate failed: {0}", e);
                }
            };

            NativeAPI.HealthCheckCallback handleHealthCallback = () =>
            {
                try
                {
                    Logger.Info("GameLiftAPI] onHealthCheck called");
                    return onHealthCheck();
                }
                catch (Exception e)
                {
                    Logger.Error("[GameLiftAPI] onHealthCheck  failed: {0}", e);
                    return false;
                }
            };

            // Make the Native Call
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_ProcessReady(handleSessionStartCallback,
                handleProcessTerminateCallback,
                handleHealthCallback,
                port,
                logPath);

            return ReadGenericOutcome("ProcessReady", pOutcome);
        }

        public static bool ProcessEnding()
        {
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_ProcessEnding();
            return ReadGenericOutcome("ProcessEnding", pOutcome);
        }

        public static bool ActivateGameSession()
        {
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_ActivateGameSession();
            return ReadGenericOutcome("ProcessEnding", pOutcome);
        }

        public static bool TerminateGameSession()
        {
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_TerminateGameSession();
            return ReadGenericOutcome("TerminateGameSession", pOutcome);
        }

        public static string GetGameSessionId()
        {
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_GetGameSessionId();
            return ReadStringOutcome("GetGameSessionId", pOutcome);
        }

        public static bool AcceptPlayerSession(string playerSession)
        {
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_AcceptPlayerSession(playerSession);
            return ReadGenericOutcome("AcceptPlayerSession", pOutcome);
        }

        public static bool RemovePlayerSession(string playerSession)
        {
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_RemovePlayerSession(playerSession);
            return ReadGenericOutcome("RemovePlayerSession", pOutcome);
        }

        public static bool Destroy()
        {
            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_Destroy();
            return ReadGenericOutcome("Destroy", pOutcome);
        }

        #region Private Methods
        private static string ReadStringOutcome(string functionName, IntPtr pOutcome)
        {
            string value = null;
            if (pOutcome != IntPtr.Zero)
            {
                StringBuilder sb = new StringBuilder(1024);

                bool success = NativeAPI.aws_gamelift__AwsStringOutcome_IsSuccess(pOutcome);
                if (success)
                {
                    NativeAPI.aws_gamelift__AwsStringOutcome_GetString(pOutcome, sb.Capacity, sb);
                    value = sb.ToString();
                }
                else
                {
                    GameLiftErrorType error = NativeAPI.aws_gamelift__AwsStringOutcome_GetErrorType(pOutcome);
                    NativeAPI.aws_gamelift__AwsStringOutcome_GetErrorMessage(pOutcome, sb.Capacity, sb);

                    throw new Exception(string.Format("[GameLiftAPI] {0} Failed. Error: {1}, Message: {2}", functionName, error, sb.ToString()));
                }

                NativeAPI.aws_gamelift__AwsStringOutcome_Delete(pOutcome);
            }
            else
            {
                Logger.Error("[GameLiftAPI] {0} Outcome object is null", functionName);
            }

            return value;
        }

        private static bool ReadGenericOutcome(string functionName, IntPtr pOutcome)
        {
            bool success = false;
            if (pOutcome != IntPtr.Zero)
            {
                StringBuilder sb = new StringBuilder(1024);

                success = NativeAPI.aws_gamelift__AwsGenericOutcome_IsSuccess(pOutcome);
                if (!success)
                {
                    GameLiftErrorType error = NativeAPI.aws_gamelift__AwsGenericOutcome_GetErrorType(pOutcome);
                    NativeAPI.aws_gamelift__AwsGenericOutcome_GetErrorMessage(pOutcome, sb.Capacity, sb);

                    Logger.Error("[GameLiftAPI] {0} Failed. Error: {1}, Message: {2}", functionName, error, sb.ToString());
                }

                NativeAPI.aws_gamelift__AwsGenericOutcome_Delete(pOutcome);
            }
            else
            {
                Logger.Error("[GameLiftAPI] {0} Outcome object is null", functionName);
            }

            return success;
        }

        private static bool ReadInitSdkOutcome(string functionName, IntPtr pOutcome)
        {
            IntPtr pServerState = IntPtr.Zero;
            if (pOutcome != IntPtr.Zero)
            {
                bool success = NativeAPI.aws_gamelift__InitSDKOutcome_IsSuccess(pOutcome);
                if (success)
                {
                    pServerState = NativeAPI.aws_gamelift__InitSDKOutcome_GetServerState(pOutcome);
                }
                else
                {
                    GameLiftErrorType error = NativeAPI.aws_gamelift__InitSDKOutcome_GetErrorType(pOutcome);
                    StringBuilder sb = new StringBuilder(1024);
                    NativeAPI.aws_gamelift__InitSDKOutcome_GetErrorMessage(pOutcome, sb.Capacity, sb);
                    string errorMessage = sb.ToString();

                    Logger.Error("[GameLiftAPI] {0} GameLiftError: {1}, Message: {2}", functionName, error, errorMessage);
                }

                NativeAPI.aws_gamelift__InitSDKOutcome_Delete(pOutcome);
            }
            else
            {
                Logger.Error("[GameLiftAPI] {0} Outcome object is null", functionName);
            }

            return pServerState != IntPtr.Zero;
        }

        private static GameSession ReadGameSession(string functionName, IntPtr pGameSession)
        {
            GameSession gameSession;
            if (pGameSession != IntPtr.Zero)
            {
                gameSession = new GameSession();
                StringBuilder sb = new StringBuilder(4096);

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
                string[] pairs = sb.ToString().Split(new[] { pairDelim }, StringSplitOptions.None);
                foreach (string pair in pairs)
                {
                    string[] tokens = pair.Split(new[] { keyValueDelim }, StringSplitOptions.None);
                    if (tokens.Length == 2)
                    {
                        dict[tokens[0]] = dict[tokens[1]];
                    }
                }
                gameSession.GameProperties = dict;
            }
            else
            {
                gameSession = null;
                Logger.Error("[GameLiftAPI] {0} GameSession object is null", functionName);
            }

            return gameSession;
        }
        #endregion
    }
}
