using System;

namespace Meziantou.PasswordManager.Api
{
    public class PasswordManagerException : Exception
    {
        public PasswordManagerException()
        {
        }

        public PasswordManagerException(string message) : base(message)
        {
        }

        public PasswordManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}