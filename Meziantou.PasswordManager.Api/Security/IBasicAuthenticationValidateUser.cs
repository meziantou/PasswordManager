using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Meziantou.PasswordManager.Api.Security
{
    public interface IBasicAuthenticationUserValidator
    {
        Task<AuthenticateResult> ValidateCredentials(HttpContext context, string login, string password);
    }
}