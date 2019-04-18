namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public class ErrorResponse
    {
        public ErrorResponse(ErrorCode code, string message)
        {
            Message = message;
            Code = code;
        }

        public string Message { get; set; }
        public ErrorCode Code { get; set; }
    }
}