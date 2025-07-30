using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace DigitalDocumentLockAPI.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Handled in API Exception Filter");

            var response = new
            {
                status = (int)HttpStatusCode.InternalServerError,
                message = context.Exception.Message
            };

            context.Result = new JsonResult(response)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
            
            context.ExceptionHandled = true;
        }
    }
}
