namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public enum ErrorCode
    {
        Unknown,
        UserAlreadyExists,
        UserNotFound,
        UserHasNotSetPublicKey,
        DocumentNotFound,
        InvalidPassword,
        Unauthorized
    }
}