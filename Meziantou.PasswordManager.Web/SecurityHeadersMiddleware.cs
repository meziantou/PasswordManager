using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Meziantou.PasswordManager.Web
{
    internal class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException("next");
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("content-security-policy", "script-src 'self' 'unsafe-inline' 'unsafe-eval'");
            context.Response.Headers.Add("x-xss-protection", "1; mode=block");
            context.Response.Headers.Add("x-frame-options", "SAMEORIGIN");
            context.Response.Headers.Add("x-content-type", "nosniff");

            return _next(context);
        }
    }
}
