namespace dainiki.Components.Services
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public AuthResult()
        {
            ErrorMessage = string.Empty;
        }

        public AuthResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}
