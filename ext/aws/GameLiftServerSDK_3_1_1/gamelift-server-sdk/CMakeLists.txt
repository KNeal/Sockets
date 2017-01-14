CMAKE_MINIMUM_REQUIRED(VERSION 2.8)
PROJECT(aws-cpp-sdk-gamelift-server)
SET(TARGET_NAME aws-cpp-sdk-gamelift-server)

if (UNIX)
   add_definitions(-std=c++11)
endif()

message("BUILD_SHARED_LIBS ${BUILD_SHARED_LIBS}")
if (BUILD_SHARED_LIBS)
  set(LIBTYPE SHARED)
else (BUILD_SHARED_LIBS)
  set(LIBTYPE STATIC)
endif (BUILD_SHARED_LIBS)

# -----------------------------
# Set up source directories
# -----------------------------
if(UNIX)
    set(GAMELIFT_SOURCE_ROOT ${CMAKE_SOURCE_DIR})
    set(3RD_PARTY_ROOT ${GAMELIFT_SOURCE_ROOT}/../3rdParty)
elseif(WIN32)
    set(GAMELIFT_SOURCE_ROOT ${CMAKE_SOURCE_DIR}/gamelift-server-sdk)
endif()
file(GLOB AWS_GAMELIFT_COMMON_HEADERS "" "${GAMELIFT_SOURCE_ROOT}/include/aws/gamelift/common/*.h*")
file(GLOB AWS_GAMELIFT_COMMON_SOURCE  "" "${GAMELIFT_SOURCE_ROOT}/source/aws/gamelift/common/*.cpp")
file(GLOB AWS_GAMELIFT_SERVER_HEADERS "" "${GAMELIFT_SOURCE_ROOT}/include/aws/gamelift/server/*.h*")
file(GLOB AWS_GAMELIFT_SERVER_SOURCE  "" "${GAMELIFT_SOURCE_ROOT}/source/aws/gamelift/server/*.cpp")
file(GLOB AWS_GAMELIFT_MODEL_HEADERS "" "${GAMELIFT_SOURCE_ROOT}/include/aws/gamelift/server/model/*.h*")
file(GLOB AWS_GAMELIFT_INTERNAL_HEADERS "" "${GAMELIFT_SOURCE_ROOT}/include/aws/gamelift/internal/*.h")
file(GLOB AWS_GAMELIFT_INTERNAL_NETWORK_HEADERS "" "${GAMELIFT_SOURCE_ROOT}/include/aws/gamelift/internal/network/*.h")
set(GAMELIFT_SERVER_SRC
   ${AWS_GAMELIFT_INTERNAL_NETWORK_HEADERS}
   ${AWS_GAMELIFT_INTERNAL_HEADERS}
   ${AWS_GAMELIFT_COMMON_HEADERS}
   ${AWS_GAMELIFT_MODEL_HEADERS}
   ${AWS_GAMELIFT_SERVER_HEADERS}
   ${AWS_GAMELIFT_COMMON_SOURCE}
   ${AWS_GAMELIFT_SERVER_SOURCE}
   ${GAMELIFT_SOURCE_ROOT}/source/aws/gamelift/server/protocols/sdk.pb.cc
)

# -----------------------------
# Set up include directories
# -----------------------------
if(UNIX)
    #socketIO
    include_directories(${3RD_PARTY_ROOT}/sioclient/src/socketIO-ext/src)

    #protobuf
    include_directories(${3RD_PARTY_ROOT}/protobuf/src/protobuf-src/src)

    include_directories(${CMAKE_SOURCE_DIR}/include)
    add_library(${TARGET_NAME} ${LIBTYPE} ${GAMELIFT_SERVER_SRC})

elseif(WIN32)
    include_directories(${CMAKE_SOURCE_DIR}/gamelift-server-sdk/include)
    include_directories(${CMAKE_SOURCE_DIR}/gamelift-server-sdk/3rdParty/include)
    
    #Generating protobuf files.
    message("Generating protbuf files.")
    set (protoc_cmd "${CMAKE_SOURCE_DIR}/gamelift-server-sdk/3rdParty/protoc")
    set (protoc_arg1 "${CMAKE_SOURCE_DIR}/gamelift-server-sdk/source/aws/gamelift/server/protocols/sdk.proto")
    set (protoc_arg2 "--cpp_out=${CMAKE_SOURCE_DIR}/gamelift-server-sdk/source")
    set (protoc_arg3 "-I=${CMAKE_SOURCE_DIR}/gamelift-server-sdk/source")
    execute_process(COMMAND ${protoc_cmd} ${protoc_arg1} ${protoc_arg2} ${protoc_arg3}
      WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/gamelift-server-sdk/source/aws/gamelift/server/protocols/
      RESULT_VARIABLE protoc_result)
    
    message("Copying sdk.pb.h to include folder.")
    execute_process(COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_SOURCE_DIR}/gamelift-server-sdk/source/aws/gamelift/server/protocols/sdk.pb.h ${CMAKE_SOURCE_DIR}/gamelift-server-sdk/include/aws/gamelift/server/protocols/sdk.pb.h)
    
    add_library(${TARGET_NAME} ${LIBTYPE} ${GAMELIFT_SERVER_SRC})
endif()


# -----------------------------
# Set up link targets
# -----------------------------

if(BUILD_SHARED_LIBS)
   add_definitions("-DUSE_IMPORT_EXPORT=1")
   if (MSVC)
      add_definitions("-DAWS_GAMELIFT_EXPORTS")
      target_link_libraries(aws-cpp-sdk-gamelift-server
                              boost_date_time boost_random boost_system
                              sioclient
                              protobuf)
      
   elseif(UNIX)
      find_library(BOOST_DATE_TIME boost_date_time HINTS ${3RD_PARTY_ROOT}/boost/src/boost-ext/stage/lib)
      find_library(BOOST_RANDOM boost_random HINTS ${3RD_PARTY_ROOT}/boost/src/boost-ext/stage/lib)
      find_library(BOOST_SYSTEM boost_system HINTS ${3RD_PARTY_ROOT}/boost/src/boost-ext/stage/lib)
      find_library(SOCKET_IO_LIB sioclient HINTS ${3RD_PARTY_ROOT}/sioclient/src/socketIO-ext-build)
      find_library(PROTOBUF_LIB protobuf HINTS ${3RD_PARTY_ROOT}/protobuf/src/protobuf-build/src/.libs)
      target_link_libraries(${TARGET_NAME}
                ${BOOST_DATE_TIME}
                ${BOOST_RANDOM}
                ${SOCKET_IO_LIB}
                ${PROTOBUF_LIB})
   endif(MSVC)
   
endif(BUILD_SHARED_LIBS)

# If we are compiling for visual studio, create a sane directory tree.
if(MSVC)
    source_group("Header Files\\aws\\gamelift\\common" FILES ${AWS_GAMELIFT_COMMON_HEADERS})
    source_group("Header Files\\aws\\gamelift\\server" FILES ${AWS_GAMELIFT_SERVER_HEADERS})
    source_group("Header Files\\aws\\gamelift\\internal" FILES ${AWS_GAMELIFT_INTERNAL_HEADERS})
    source_group("Header Files\\aws\\gamelift\\internal\\network" FILES ${AWS_GAMELIFT_INTERNAL_NETWORK_HEADERS})
    source_group("Header Files\\aws\\gamelift\\server\\model" FILES ${AWS_GAMELIFT_MODEL_HEADERS})
    source_group("Source Files\\common" FILES ${AWS_GAMELIFT_COMMON_SOURCE})
    source_group("Source Files\\server" FILES ${AWS_GAMELIFT_SERVER_SOURCE})
    source_group("Source Files\\server\\protocols" FILES ${AWS_GAMELIFT_SERVER_PROTOCOLS_SOURCE})
    add_definitions("/Zi")
    # Make the Release version create a PDB
    set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /Zi")
    # Enable Generate Debug Info for both debug and release configure
    set_property(TARGET aws-cpp-sdk-gamelift-server APPEND PROPERTY LINK_FLAGS /DEBUG)
    # Minimize the size of the resulting DLLs
    set(CMAKE_SHARED_LINKER_FLAGS_RELEASE "${CMAKE_SHARED_LINKER_FLAGS_RELEASE} /OPT:REF")
endif()
  