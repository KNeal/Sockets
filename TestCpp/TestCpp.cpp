// TestCpp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <aws/gamelift/server/GameLiftServerAPI.h>


int _tmain(int argc, _TCHAR* argv[])
{
	Aws::GameLift::Server::InitSDK();

	return 0;
}

