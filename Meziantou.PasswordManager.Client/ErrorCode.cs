namespace Meziantou.PasswordManager.Client
{
    public enum ErrorCode
    {
        Unknown,
        UserAlreadyExists,
        UserNotFound,
        UserHasNotSetPublicKey,

        InvalidMasterKey = 10000,
        NoKeyFound = 10001,
    }
}