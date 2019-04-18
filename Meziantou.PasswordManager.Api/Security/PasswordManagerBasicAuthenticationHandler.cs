using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Meziantou.PasswordManager.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Meziantou.PasswordManager.Api.Security
{
    internal class PasswordManagerBasicAuthenticationUserValidator : IBasicAuthenticationUserValidator
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly UserRepository _userRepository;

        public PasswordManagerBasicAuthenticationUserValidator(UserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<AuthenticateResult> ValidateCredentials(HttpContext context, string login, string password)
        {
            var user = await _userRepository.LoadByUsernameAsync(login);
            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(user.Password, password);
                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.Password = _passwordHasher.HashPassword(password);
                    await _userRepository.SaveAsync(user);
                }

                if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    var id = new ClaimsIdentity(BasicAuthenticationOptions.AuthenticationScheme, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                    id.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.Username, "http://www.w3.org/2001/XMLSchema#string"));
                    id.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", user.Username, "http://www.w3.org/2001/XMLSchema#string"));
                    id.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Meziantou.PasswordManager", "http://www.w3.org/2001/XMLSchema#string"));

                    return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(id), new AuthenticationProperties(), BasicAuthenticationOptions.AuthenticationScheme));
                }
            }

            return AuthenticateResult.Fail("Invalid username or password.");
        }
    }
}