using System;
using Microsoft.AspNetCore.Authentication;

namespace Meziantou.PasswordManager.Api.Security
{
    internal static class BasicAuthenticationExtensions
    {
        public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder)
            => builder.AddBasicAuthentication(BasicAuthenticationOptions.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder, Action<BasicAuthenticationOptions> configureOptions)
            => builder.AddBasicAuthentication(BasicAuthenticationOptions.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<BasicAuthenticationOptions> configureOptions)
            => builder.AddBasicAuthentication(authenticationScheme, null, configureOptions);

        public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BasicAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}