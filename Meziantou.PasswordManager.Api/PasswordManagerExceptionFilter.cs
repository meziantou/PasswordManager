using System;
using Meziantou.PasswordManager.Api.ServiceModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Meziantou.PasswordManager.Api
{
    public class PasswordManagerExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;

        public PasswordManagerExceptionFilter(ILogger<PasswordManagerExceptionFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception.ToString());

            var exceptionType = context.Exception.GetType();
            if (exceptionType == typeof(NotImplementedException))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status501NotImplemented);
                context.ExceptionHandled = true;
            }
            else
            {
                context.Result = new ErrorResult(new ErrorResponse(ErrorCode.Unknown, context.Exception.Message));
                context.ExceptionHandled = true;
            }
        }

        private class ErrorResult : ObjectResult
        {
            public ErrorResult(object value)
                : base(value)
            {
                StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}