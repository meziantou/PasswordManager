using System;

namespace Meziantou.PasswordManager.Client
{
    public class UserNameAndPublicKey
    {
        public UserNameAndPublicKey(string username, string publicKey)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (publicKey == null) throw new ArgumentNullException(nameof(publicKey));

            Username = username;
            PublicKey = publicKey;
        }

        public string Username { get; set; }
        public string PublicKey { get; set; }
    }
}