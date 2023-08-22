using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Example.Messaging.MassTransit
{

    public class MassTransitExceptionFilter : Microsoft.AspNetCore.Mvc.Filters.IExceptionFilter
    {
        private readonly ILogger<MassTransitExceptionFilter> _logger;

        public MassTransitExceptionFilter(ILogger<MassTransitExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is RequestFaultException ex)
            {
                _logger.LogError(ex, "An unexpected exception occurred whilst processing a MassTransit message.");

                context.Result = new BadRequestObjectResult(new
                {
                    status = 400,
                    error = "Bad Request",
                    message = "An unexpected exception occurred whilst processing a MassTransit message."
                });
            }

            context.ExceptionHandled = true;
        }
    }

}
