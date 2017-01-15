#include <aws/gamelift/server/GameLiftServerAPI.h>
#include <aws/gamelift/server/GameLiftServerAPI_extern_c.h>
#include <aws/gamelift/common/Outcome.h>
#include <iostream>
#include <fstream>
#include <map>

using namespace Aws::GameLift;
using namespace Aws::GameLift::Server;
using namespace Aws::GameLift::Server::Model;

// TODO... does this need a lock?
// TODO... error handling
// Hold onto references of the data so they may be accssed multiple times from managed code.

template<typename T>
class Cache
{
private:
	std::map<void*, std::shared_ptr<T>> mCache;

public:
	void* Add(T value)
	{
		std::shared_ptr<T> ptr = std::make_shared<T>(value);
		mCache[ptr.get()] = ptr;
		return ptr.get();
	}

	void Remove(void * id)
	{
		auto iter = mCache.find((void*)id);
		if (iter != mCache.end())
		{
			iter->second = NULL;
			mCache.erase(iter);
		}
	}
};

void Log(char* message)
{
	std::ofstream log("logfile.txt", std::ios_base::app | std::ios_base::out);
	log << message << std::endl;
}

Cache<AwsStringOutcome> s_cacheAwsStringOutcome;
Cache<InitSDKOutcome> s_cacheInitSDKOutcome;
Cache<GenericOutcome> s_cacheGenericOutcome;

extern "C" extern AWS_GAMELIFT_API void test_callback(fnTestCallback callback)
{
	callback("Test String Result");
}

// ---------------------------------------------------------------------------
// Server API
// ---------------------------------------------------------------------------

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_GetSdkVersion()
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_GetSdkVersion called");
	AwsStringOutcome outcome = Server::GetSdkVersion();

	return s_cacheAwsStringOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_InitSDK()
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_InitSDK called");
	InitSDKOutcome outcome = Aws::GameLift::Server::InitSDK();

	return s_cacheInitSDKOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_ProcessReady
(	
	FuncOnGameSessionStart onGameStart,
	FuncOnProcessTerminate onProcessTerminate,
	FuncOnHealthCheck onHealthCheck,
	int port,
	char* pLogPath
)
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_ProcessReady called");

	// Wrap the callbacks in functions
	std::function<void(GameSession)> fonGameStart = [onGameStart](Aws::GameLift::Server::Model::GameSession gameSession)
	{
		Log("[GameLiftServerAPI_extern_c] onGameStart called");
		onGameStart(&gameSession);
	};

	std::function<void()> fonProcessTerminate = [onProcessTerminate]()
	{
		Log("[GameLiftServerAPI_extern_c] onProcessTerminate called");
		onProcessTerminate();
	};

	std::function<bool()> fonHealthCheck = [onHealthCheck]()
	{
		Log("[GameLiftServerAPI_extern_c] onHealthCheck called");
		return onHealthCheck();
	};

	std::vector<std::string> logPaths;
	logPaths.push_back(std::string(pLogPath));
	LogParameters logParams(logPaths);

	ProcessParameters processParams(fonGameStart, fonProcessTerminate, fonHealthCheck, port, logParams);

	GenericOutcome outcome = Server::ProcessReady(processParams);
	return s_cacheGenericOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_ProcessEnding()
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_ProcessEnding called");
	GenericOutcome outcome = Server::ProcessEnding();
	return s_cacheGenericOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_ActivateGameSession()
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_ActivateGameSession called");
	GenericOutcome outcome = Server::ActivateGameSession();
	return s_cacheGenericOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_TerminateGameSession()
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_TerminateGameSession called");
	GenericOutcome outcome = Server::TerminateGameSession();
	return s_cacheGenericOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_GetGameSessionId()
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_GetGameSessionId called");
	AwsStringOutcome outcome = Server::GetGameSessionId();

	return s_cacheAwsStringOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_AcceptPlayerSession(char *pPlayerSession)
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_AcceptPlayerSession called");
	GenericOutcome outcome = Server::AcceptPlayerSession(pPlayerSession);
	return s_cacheGenericOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_RemovePlayerSession(char *pPlayerSession)
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_RemovePlayerSession called");
	GenericOutcome outcome = Server::RemovePlayerSession(pPlayerSession);
	return s_cacheGenericOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_Destroy()
{
	Log("[GameLiftServerAPI_extern_c] aws_gamelift_server__API_Destroy called");
	GenericOutcome outcome = Server::Destroy();
	return s_cacheGenericOutcome.Add(outcome);
}

// ---------------------------------------------------------------------------
// GenericOutcome
// ---------------------------------------------------------------------------

extern "C" AWS_GAMELIFT_API void aws_gamelift__AwsGenericOutcome_Delete(Aws::GameLift::GenericOutcome* pOutcome)
{
	s_cacheGenericOutcome.Remove(pOutcome);
}

extern "C" AWS_GAMELIFT_API bool aws_gamelift__AwsGenericOutcome_IsSuccess(Aws::GameLift::GenericOutcome* pOutcome)
{
	return pOutcome->IsSuccess();
}

extern "C" AWS_GAMELIFT_API int	aws_gamelift__AwsGenericOutcome_GetErrorType(Aws::GameLift::GenericOutcome* pOutcome)
{
	return (int)pOutcome->GetError().GetErrorType();
}

extern "C" AWS_GAMELIFT_API void aws_gamelift__AwsGenericOutcome_GetErrorMessage(Aws::GameLift::GenericOutcome* pOutcome, int bufferLen, char* pBuffer)
{
	const char* pStr = pOutcome->GetError().GetErrorMessage().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

// ---------------------------------------------------------------------------
// AwsStringOutcome
// ---------------------------------------------------------------------------

extern "C" extern AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_Delete(Aws::GameLift::AwsStringOutcome* pOutcome)
{
	s_cacheAwsStringOutcome.Remove(pOutcome);
}

extern "C" AWS_GAMELIFT_API bool  aws_gamelift__AwsStringOutcome_IsSuccess(Aws::GameLift::AwsStringOutcome* pOutcome)
{
	return pOutcome->IsSuccess();
}

extern "C" AWS_GAMELIFT_API int aws_gamelift__AwsStringOutcome_GetErrorType(Aws::GameLift::AwsStringOutcome* pOutcome)
{
	return (int)pOutcome->GetError().GetErrorType();
}

extern "C" AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_GetErrorMessage(Aws::GameLift::AwsStringOutcome* pOutcome, int bufferLen, char* pBuffer)
{
	const char* pStr = pOutcome->GetError().GetErrorMessage().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

extern "C" AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_GetString(Aws::GameLift::AwsStringOutcome* pOutcome, int bufferLen, char* pBuffer)
{
	const char* pStr = pOutcome->GetResult().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

// ---------------------------------------------------------------------------
// InitSDKOutcome
// ---------------------------------------------------------------------------

extern "C" extern AWS_GAMELIFT_API void aws_gamelift__InitSDKOutcome_Delete(Aws::GameLift::Server::InitSDKOutcome* pOutcome)
{
	s_cacheInitSDKOutcome.Remove(pOutcome);
}

extern "C" AWS_GAMELIFT_API bool aws_gamelift__InitSDKOutcome_IsSuccess(Aws::GameLift::Server::InitSDKOutcome* pOutcome)
{
	return pOutcome->IsSuccess();
}


extern "C" AWS_GAMELIFT_API int aws_gamelift__InitSDKOutcome_GetErrorType(Aws::GameLift::Server::InitSDKOutcome* pOutcome)
{
	return (int)pOutcome->GetError().GetErrorType();
}

extern "C" AWS_GAMELIFT_API void aws_gamelift__InitSDKOutcome_GetErrorMEssage(Aws::GameLift::Server::InitSDKOutcome* pOutcome, int bufferLen, char* pBuffer)
{
	const char* pStr = pOutcome->GetError().GetErrorMessage().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift__InitSDKOutcome_GetServerState(Aws::GameLift::Server::InitSDKOutcome* pOutcome)
{
	void *pState = pOutcome->GetResult();
	return pState;
}

// ---------------------------------------------------------------------------
// GameSession
// ---------------------------------------------------------------------------

extern "C" AWS_GAMELIFT_API void aws_gamelift__GameSession_GetGameSessionId(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer)
{
	const char* pStr = pGameSession->GetGameSessionId().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

extern "C" AWS_GAMELIFT_API void aws_gamelift__GameSession_GetName(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer)
{
	const char* pStr = pGameSession->GetName().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

extern "C" AWS_GAMELIFT_API void aws_gamelift__GameSession_GetFleetId(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer)
{
	const char* pStr = pGameSession->GetFleetId().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

extern "C" AWS_GAMELIFT_API void aws_gamelift__GameSession_GetIpAddress(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer)
{
	const char* pStr = pGameSession->GetIpAddress().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

extern "C" AWS_GAMELIFT_API int  aws_gamelift__GameSession_GetPort(Aws::GameLift::Server::Model::GameSession* pGameSession)
{
	return pGameSession->GetPort();
}

extern "C" AWS_GAMELIFT_API int  aws_gamelift__GameSession_GetMaximumPlayerSessionCount(Aws::GameLift::Server::Model::GameSession* pGameSession)
{
	return pGameSession->GetMaximumPlayerSessionCount();
}

extern "C" AWS_GAMELIFT_API void  aws_gamelift__GameSession_GetGamePropertiesString(Aws::GameLift::Server::Model::GameSession* pGameSession, int bufferLen, char* pBuffer, char *pKeyValueDelim, char* pPairDelim)
{	
	auto gameProp = pGameSession->GetGameProperties();
	std::string data;
	for (int i = 0; i < gameProp.size(); ++i)
	{
		data.append(gameProp[i].GetKey());
		data.append(pKeyValueDelim);
		data.append(gameProp[i].GetValue());
		data.append(pPairDelim);
	};

	strncpy(pBuffer, data.c_str(), bufferLen);
}
