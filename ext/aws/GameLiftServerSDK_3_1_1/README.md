Thank you for dowloading Amazon GameLift Server SDK.

Documentation for Amazon GameLift is located at:
https://aws.amazon.com/documentation/gamelift/


This package includes the source code for the GameLift Server SDK. Use this SDK to integrate Amazon GameLift into your game servers.

Use the AWS SDK for GameLift to integrate your game clients and access functionality to manage your fleets and builds. Please visit https://github.com/aws/aws-sdk-cpp to access the AWS C++ SDK clients, including aws-cpp-sdk-gamelift and aws-cpp-sdk-core.

### Building the GameLift Server SDK
Use the information below to build the entire source tree for your platform.

#### Minimum Requirements:
* Visual Studio 2013 or later
* OR GNU Compiler Collection(GCC) 4.9 or later

#### Creating a Out-of-Source Build (Recommended)
To create an **out-of-source build**:

1.Install CMake and the relevant build tools for your platform. Ensure these are available in your executable path.  
2.Create your build directory (replace BUILD_DIR with your build directory name):

```
 cd BUILD_DIR
```

3.Create a build:

To create a **static release build**, do one of the following:  
- For Visual Studio:
```
 cmake <path-to-root-of-this-source-code> -G "Visual Studio 12 Win64" -DBUILD_SHARED_LIBS=OFF
 msbuild ALL_BUILD.vcxproj /p:Configuration=Release
```
- For Auto Make build systems:
```
 cmake <path-to-root-of-this-source-code> -G "Unix Makefiles" -DBUILD_SHARED_LIBS=OFF
 make
```

#### Note on 3rdParty libraries
GameLift Server SDK has three dependencies:
1. [Boost (system, date_time and random)](http://www.boost.org/)
2. [SIOClient](https://github.com/socketio/socket.io-client-cpp)
3. [Protobuf](https://github.com/google/protobuf)

This package contains 3rdParty libraries for Windows (msvc12).  
On Linux platforms, dependencies will be downloaded and built alongside the GameLift Server SDK when running 'make'.