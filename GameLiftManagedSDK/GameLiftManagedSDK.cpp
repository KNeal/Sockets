// This is the main DLL file.

#include "stdafx.h"

#include "GameLiftManagedSDK.h"
#include <aws/gamelift/server/GameLiftServerAPI.h>

using namespace Aws::GameLift;
using namespace Aws::GameLift::Server;

namespace GameLiftManagedSDK
{
	String^ GameLiftServerAPI::GetSDKVersion()
	{
		AwsStringOutcomeSharedPtr outcome = GameLiftServerAPI::GetSDKVersion();
		String^ versionStr = gcnew String(outcome->GetResult().c_str());
		return versionStr;
	}
}