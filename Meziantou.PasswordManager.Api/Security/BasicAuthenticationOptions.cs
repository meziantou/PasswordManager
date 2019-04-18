using Microsoft.AspNetCore.Authentication;

namespace Meziantou.PasswordManager.Api.Security
{
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "Basic";

        public string Realm { get; set; }
        public IBasicAuthenticationUserValidator ValidateUser { get; set; }
    }
}