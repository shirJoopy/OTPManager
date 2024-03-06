using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace OTPManager.Filters
{

    public class AuditActionFilter : IActionFilter, IAsyncResultFilter, IAsyncExceptionFilter
    {
        private readonly ILogger<AuditActionFilter> _logger;

        public AuditActionFilter(ILogger<AuditActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Log details about the action, its arguments, and request headers
            _logger.LogInformation($"Executing action: {context.ActionDescriptor.DisplayName}");

            foreach (var argument in context.ActionArguments)
            {
                var argumentValue = JsonSerializer.Serialize(argument.Value);
                _logger.LogInformation($"Argument: {argument.Key} = {argumentValue}");
            }

            // Log request headers
            foreach (var header in context.HttpContext.Request.Headers)
            {
                _logger.LogInformation($"Header: {header.Key} = {string.Join(", ", header.Value)}");
            }
        }


        public void OnActionExecuted(ActionExecutedContext context)
        {
            // This method is invoked after the action method is called but before the action result is executed.
            // Logging here won't give you the result status code since the result hasn't been executed yet.

        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {

            // Execute the action result
            var resultExecutedContext = await next();

            // After the result is executed, log the status code
            _logger.LogInformation($"Action {context.ActionDescriptor.DisplayName} executed with status code: {context.HttpContext.Response.StatusCode}");


        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            // Log the exception
            _logger.LogError(context.Exception, "An exception occurred in action {ActionName}", context.ActionDescriptor.DisplayName);

            // Optionally, you can set the result to customize the response, thereby preventing the exception from propagating further.
            // For example, to return a generic error response:
            // context.Result = new ObjectResult("An error occurred") { StatusCode = 500 };
            // context.ExceptionHandled = true; // Prevents the exception from propagating further

            await Task.CompletedTask;
        }
    }
}
