namespace AWS.GameLift
{
    public enum GameLiftErrorType
    {
        ALREADY_INITIALIZED,            // The GameLift Server or Client has already been initialized with Initialize().
        FLEET_MISMATCH,                 // The target fleet does not match the fleet of a gameSession or playerSession.
        GAMELIFT_CLIENT_NOT_INITIALIZED,// The GameLift client has not been initialized.
        GAMELIFT_SERVER_NOT_INITIALIZED,//The GameLift server has not been initialized.
        GAME_SESSION_ENDED_FAILED,      // The GameLift Server SDK could not contact the service to report the game session ended.
        GAME_SESSION_NOT_READY,         // The GameLift Server Game Session was not activated.
        GAME_SESSION_READY_FAILED,      // The GameLift Server SDK could not contact the service to report the game session is ready.
        INITIALIZATION_MISMATCH,        // A client method was called after Server::Initialize(), or vice versa.
        NOT_INITIALIZED,                // The GameLift Server or Client has not been initialized with Initialize().
        NO_TARGET_ALIASID_SET,          // A target aliasId has not been set.
        NO_TARGET_FLEET_SET,            // A target fleet has not been set.
        PROCESS_ENDING_FAILED,          // The GameLift Server SDK could not contact the service to report the process is ending.
        PROCESS_NOT_ACTIVE,             // The server process is not yet active, not bound to a GameSession, and cannot accept or process PlayerSessions.
        PROCESS_NOT_READY,              // The server process is not yet ready to be activated.
        PROCESS_READY_FAILED,           // The GameLift Server SDK could not contact the service to report the process is ready.
        SDK_VERSION_DETECTION_FAILED,   // SDK version detection failed.
        SERVICE_CALL_FAILED,            // A call to an AWS service has failed.
        STX_CALL_FAILED,                // A call to the XStx server backend component has failed.
        STX_INITIALIZATION_FAILED,      // The XStx server backend component has failed to initialize.
        UNEXPECTED_PLAYER_SESSION       // An unregistered player session was encountered by the server.
    };
}