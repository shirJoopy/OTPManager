using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OTPManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OTPManager.Filters
{
    public class SendSMSTotp : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next(); // Ensure action execution completes

            if (executedContext.Result is OkObjectResult)
            {
                var httpContext = context.HttpContext;
                var logger = httpContext.RequestServices.GetRequiredService<ILogger<SendSMSTotp>>();

                try
                {
                    httpContext.Items.TryGetValue("SEND_SMS", out var shouldSendSmsObj);
                    bool.TryParse((shouldSendSmsObj ?? "false").ToString(), out var shouldSendSMS);
                    if (shouldSendSMS)
                    {
                        if (httpContext.Items.TryGetValue("userDetails", out var userDetailsObj) &&
                            userDetailsObj is Dictionary<string, string> details &&
                            details.TryGetValue("PHONE_NUMBER", out var to))
                        {
                            var smsService = httpContext.RequestServices.GetRequiredService<ISmsService>();
                            var body = $"Your one time code is {httpContext.Items["TOTP"]}";
                            smsService.SendSmsAsync(to, body); // Properly await async call
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send SMS");
                    // Consider how you want to handle failures. Perhaps set a specific result on the context?
                }
            }
        }
    }
}
