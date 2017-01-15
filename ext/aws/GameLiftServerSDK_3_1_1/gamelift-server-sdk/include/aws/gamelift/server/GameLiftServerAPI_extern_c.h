#pragma once
#include <aws/gamelift/common/GameLift_EXPORTS.h>
#include <aws/gamelift/common/Outcome.h>

extern "C"
{
	// Testing
	typedef void(__stdcall *fnTestCallback)(std::string result);
	extern AWS_GAMELIFT_API void test_callback(fnTestCallback callback);

	// Callbacks
	typedef void(__stdcall *FuncOnGameSessionStart)(Aws::GameLift::Server::Model::GameSession* pGameSession);
	typedef void(__stdcall *FuncOnProcessTerminate)();
	typedef bool(__stdcall *FuncOnHealthCheck)();

	// API
	extern AWS_GAMELIFT_API void* aws_gamelift_server__API_GetSdkVersion();
	extern AWS_GAMELIFT_API void* aws_gamelift_server__API_InitSDK();	
	extern AWS_GAMELIFT_API void* aws_gamelift_server__API_ProcessReady(FuncOnGameSessionStart onGameStart, 
																		FuncOnProcessTerminate onProcessTerminate, 
																		FuncOnHealthCheck onHealthCheck, 
																		int port, char* pLogPath);

	// GenericOutcome
	extern AWS_GAMELIFT_API void aws_gamelift__AwsGenericOutcome_Delete(Aws::GameLift::GenericOutcome* pOutcome);
	extern AWS_GAMELIFT_API bool aws_gamelift__AwsGenericOutcome_IsSuccess(Aws::GameLift::GenericOutcome* pOutcome);
	extern AWS_GAMELIFT_API int	 aws_gamelift__AwsGenericOutcome_GetErrorType(Aws::GameLift::GenericOutcome* pOutcome);
	extern AWS_GAMELIFT_API void aws_gamelift__AwsGenericOutcome_GetErrorMessage(Aws::GameLift::GenericOutcome* pOutcome, int bufferLen, char* pBuffer);

	// AwsStringOutcome
	extern AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_Delete(Aws::GameLift::AwsStringOutcome* pOutcome);
	extern AWS_GAMELIFT_API bool aws_gamelift__AwsStringOutcome_IsSuccess(Aws::GameLift::AwsStringOutcome* pOutcome);
	extern AWS_GAMELIFT_API int	 aws_gamelift__AwsStringOutcome_GetErrorType(Aws::GameLift::AwsStringOutcome* pOutcome);
	extern AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_GetErrorMessage(Aws::GameLift::AwsStringOutcome* pOutcome, int bufferLen, char* pBuffer);
	extern AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_GetString(Aws::GameLift::AwsStringOutcome* pOutcome, int bufferLen, char* pBuffer);

	// InitSDKOutcome
	extern AWS_GAMELIFT_API void  aws_gamelift__InitSDKOutcome_Delete(Aws::GameLift::Server::InitSDKOutcome* pOutcome);
	extern AWS_GAMELIFT_API bool  aws_gamelift__InitSDKOutcome_IsSuccess(Aws::GameLift::Server::InitSDKOutcome* pOutcome);
	extern AWS_GAMELIFT_API int   aws_gamelift__InitSDKOutcome_GetErrorType(Aws::GameLift::Server::InitSDKOutcome* pOutcome);
	extern AWS_GAMELIFT_API void  aws_gamelift__InitSDKOutcome_GetErrorMessage(Aws::GameLift::Server::InitSDKOutcome* pOutcome, int bufferLen, char* pBuffer);
	extern AWS_GAMELIFT_API void* aws_gamelift__InitSDKOutcome_GetServerState(Aws::GameLift::Server::InitSDKOutcome* pOutcome);

	// GameSession
	extern AWS_GAMELIFT_API void aws_gamelift__GameSession_GetGameSessionId(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer);
	extern AWS_GAMELIFT_API void aws_gamelift__GameSession_GetName(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer);
	extern AWS_GAMELIFT_API void aws_gamelift__GameSession_GetFleetId(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer);
	extern AWS_GAMELIFT_API void aws_gamelift__GameSession_GetIpAddress(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer);
	extern AWS_GAMELIFT_API int  aws_gamelift__GameSession_GetPort(Aws::GameLift::Server::Model::GameSession* pGameSession);
	extern AWS_GAMELIFT_API int  aws_gamelift__GameSession_GetMaximumPlayerSessionCount(Aws::GameLift::Server::Model::GameSession* pGameSession);
	extern AWS_GAMELIFT_API void aws_gamelift__GameSession_GetGamePropertiesString(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer, char *pKeyValueDelim, char* pPairDelim);

}