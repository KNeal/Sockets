#pragma once
#include <aws/gamelift/common/GameLift_EXPORTS.h>
#include <aws/gamelift/common/Outcome.h>

extern "C"
{
	// API
	extern AWS_GAMELIFT_API void* aws_gamelift_server__API_GetSdkVersion();

	// AwsStringOutcome
	extern AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_Delete(Aws::GameLift::AwsStringOutcome* pOutcome);
	extern AWS_GAMELIFT_API bool aws_gamelift__AwsStringOutcome_IsSuccess(Aws::GameLift::AwsStringOutcome* pOutcome);
	extern AWS_GAMELIFT_API void aws_gamelift__AwsStringOutcome_GetResult(Aws::GameLift::AwsStringOutcome* pOutcome, int bufferLen, char* pBuffer);


}