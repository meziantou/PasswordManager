using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Meziantou.PasswordManager.Api.Security
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        public BasicAuthenticationHandler(IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorizationHeaderValue = Request.Headers.GetCommaSeparatedValues("Authorization");
            foreach (var headerValue in authorizationHeaderValue)
            {
                if (AuthenticationHeaderValue.TryParse(headerValue, out AuthenticationHeaderValue authenticationValue))
                {
                    if (string.Equals(authenticationValue.Scheme, "basic", StringComparison.OrdinalIgnoreCase))
                    {
                        return ValidateHeader(authenticationValue.Parameter);
                    }
                }
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected Task<AuthenticateResult> ValidateHeader(string authHeader)
        {
            // Decode the authentication header & split it
            var fromBase64String = Convert.FromBase64String(authHeader);
            var lp = Encoding.GetEncoding( /*28591*/ "ISO-8859-1").GetString(fromBase64String);
            if (string.IsNullOrWhiteSpace(lp))
                return null;

            string login;
            string password;
            var pos = lp.IndexOf(':');
            if (pos < 0)
            {
                login = lp;
                password = string.Empty;
            }
            else
            {
                login = lp.Substring(0, pos).Trim();
                password = lp.Substring(pos + 1).Trim();
            }

            return ValidateCredentials(Context, login, password);
        }

        protected virtual Task<AuthenticateResult> ValidateCredentials(HttpContext context, string login, string password)
        {
            if (Options.ValidateUser != null)
            {
                return Options.ValidateUser.ValidateCredentials(context, login, password);
            }

            var validator = context.RequestServices.GetService<IBasicAuthenticationUserValidator>();
            if (validator != null)
            {
                return validator.ValidateCredentials(context, login, password);
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.Headers.Append("WWW-Authenticate", BasicAuthenticationOptions.AuthenticationScheme + " realm=" + (Options.Realm ?? GetRealm()));
            return Task.CompletedTask;
        }

        protected virtual string GetRealm()
        {
            var request = Request;
            return request.Scheme + "://" + request.Host.Value;
        }
    }
}