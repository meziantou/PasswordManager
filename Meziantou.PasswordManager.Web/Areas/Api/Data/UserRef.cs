using System;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    [JsonConverter(typeof(UserRefConverter))]
    public struct UserRef : IEquatable<UserRef>
    {
        public string Email { get; }

        public UserRef(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            Email = user.Email;
        }

        public UserRef(string email)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
        }

        public static implicit operator UserRef(User user)
        {
            return new UserRef(user);
        }

        public static bool operator ==(UserRef ref1, UserRef ref2)
        {
            return ref1.Equals(ref2);
        }

        public static bool operator !=(UserRef ref1, UserRef ref2)
        {
            return !(ref1 == ref2);
        }

        public override bool Equals(object obj)
        {
            return obj is UserRef && Equals((UserRef)obj);
        }

        public bool Equals(UserRef other)
        {
            return string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Email);
        }
    }
}
