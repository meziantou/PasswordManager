using System;

namespace Meziantou.PasswordManager.Api.ServiceModel
{
    public class User
    {
        public User()
        {
        }

        public User(Data.User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            Username = user.Username;
            PublicKey = user.PublicKey;
            PrivateKey = user.PrivateKey;
            Version = user.Version;
        }

        public string Username { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public int Version { get; set; }
    }
}