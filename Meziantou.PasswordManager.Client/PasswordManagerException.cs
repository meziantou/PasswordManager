using System;

namespace Meziantou.PasswordManager.Client
{
    public class PasswordManagerException : Exception
    {
        public PasswordManagerException(ErrorCode code)
        {
            Code = code;
        }

        public PasswordManagerException(ErrorCode code, string message) : base(message)
        {
            Code = code;
        }

        public PasswordManagerException(ErrorCode code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        public ErrorCode Code { get; set; }
    }
}