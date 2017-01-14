using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AWS.GameLift
{
    public static class GameLiftAPI
    {
        private static class NativeAPI
        {
            private const string DLL_NAME = "aws-cpp-sdk-gamelift-server";
            
            // API
            [DllImport(DLL_NAME)]
            public static extern IntPtr aws_gamelift_server__API_GetSdkVersion();

            // AwsStringOutcome
            [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__AwsStringOutcome_Delete(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern bool aws_gamelift__AwsStringOutcome_IsSuccess(IntPtr pOutcome);
            [DllImport(DLL_NAME)]
            public static extern void aws_gamelift__AwsStringOutcome_GetResult(IntPtr pOutcome, int bufferLen, StringBuilder buffer);

        }


        public static string GetSdkVersion()
        {
            string version = null;

            IntPtr pOutcome = NativeAPI.aws_gamelift_server__API_GetSdkVersion();
            if (pOutcome != IntPtr.Zero)
            {
                bool success = NativeAPI.aws_gamelift__AwsStringOutcome_IsSuccess(pOutcome);
                if (success)
                {
                    StringBuilder sb = new StringBuilder(100);
                    NativeAPI.aws_gamelift__AwsStringOutcome_GetResult(pOutcome, sb.Capacity, sb);
                    version = sb.ToString();
                }

                NativeAPI.aws_gamelift__AwsStringOutcome_Delete(pOutcome);
            }

            return version;
        }

        /*
        typedef Aws::GameLift::Outcome<Aws::GameLift::Internal::GameLiftServerState*, GameLiftError> InitSDKOutcome;
    typedef Aws::GameLift::Outcome<std::vector<std::string>, GameLiftError> GetExpectedPlayerSessionIDsOutcome;
    typedef Aws::GameLift::Outcome<std::vector<std::string>, GameLiftError> GetConnectedPlayerSessionIDsOutcome;


    AWS_GAMELIFT_API AwsStringOutcome GetSdkVersion();
        */
   
    }


}
