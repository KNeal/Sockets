#include <aws/gamelift/server/GameLiftServerAPI.h>
#include <aws/gamelift/server/GameLiftServerAPI_extern_c.h>
#include <aws/gamelift/common/Outcome.h>
#include <iostream>
#include <fstream>
#include <map>

// TODO... does this need a lock?
std::map<void*, std::shared_ptr<Aws::GameLift::AwsStringOutcome>> s_OutcomeCache;

// ---------------------------------------------------------------------------
// aws_gamelift_server_API
// ---------------------------------------------------------------------------

extern "C" AWS_GAMELIFT_API void* aws_gamelift_server__API_GetSdkVersion()
{
	Aws::GameLift::AwsStringOutcomeSharedPtr outcome = Aws::GameLift::Server::GetSdkVersion();

	s_OutcomeCache[outcome.get()] = outcome;
	return outcome.get();
}

// ---------------------------------------------------------------------------
// aws_gamelift_server_API
// ---------------------------------------------------------------------------

extern "C" extern AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_Delete(Aws::GameLift::AwsStringOutcome* pOutcome)
{
	auto iter = s_OutcomeCache.find((void*)pOutcome);
	if (iter != s_OutcomeCache.end())
	{
		iter->second = NULL;
		s_OutcomeCache.erase(iter);
	}
}

extern "C" AWS_GAMELIFT_API bool  aws_gamelift__AwsStringOutcome_IsSuccess(Aws::GameLift::AwsStringOutcome* pOutcome)
{
	return pOutcome->IsSuccess();
}

extern "C" AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_GetResult(Aws::GameLift::AwsStringOutcome* pOutcome, int bufferLen, char* pBuffer)
{
	const char* pStr = pOutcome->GetResult().c_str();
	strncpy(pBuffer, pStr, bufferLen);
}

// Aws::GameLift::Server
