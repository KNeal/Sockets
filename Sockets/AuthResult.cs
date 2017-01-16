namespace Sockets
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }

        public static AuthResult Pass()
        {
            return new AuthResult {Success = true};
        }

        public static AuthResult Fail(string message)
        {
            return new AuthResult {Success = false, Error = message};
        }
    }
}