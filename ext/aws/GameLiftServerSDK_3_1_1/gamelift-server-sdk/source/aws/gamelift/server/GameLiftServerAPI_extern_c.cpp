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

Cache<AwsStringOutcome> s_cacheAwsStringOutcome;
Cache<InitSDKOutcome> s_cacheInitSDKOutcome;
Cache<GenericOutcome> s_cacheGenericOutcome;

extern "C" extern AWS_GAMELIFT_API void test_callback(fnTestCallback callback)
{
	callback("Test String Result");
}

// ---------------------------------------------------------------------------
// aws_gamelift_server_API
// ---------------------------------------------------------------------------

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_GetSdkVersion()
{
	AwsStringOutcome outcome = Server::GetSdkVersion();

	return s_cacheAwsStringOutcome.Add(outcome);
}

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_InitSDK()
{
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
	std::function<void(GameSession)> fonGameStart = [onGameStart](Aws::GameLift::Server::Model::GameSession gameSession)
	{
		onGameStart(&gameSession);
	};

	std::function<void()> fonProcessTerminate = [onProcessTerminate]()
	{
		onProcessTerminate();
	};

	std::function<bool()> fonHealthCheck = [onHealthCheck]()
	{
		return onHealthCheck();
	};

	std::vector<std::string> logPaths;
	logPaths.push_back(std::string(pLogPath));
	LogParameters logParams(logPaths);

	ProcessParameters processParams(fonGameStart, fonProcessTerminate, fonHealthCheck, port, logParams);

	GenericOutcome outcome = Server::ProcessReady(processParams);

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
// aws_gamelift__AwsStringOutcome
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
// aws_gamelift__InitSDKOutcome
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
// aws_gamelift__GameSession
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
