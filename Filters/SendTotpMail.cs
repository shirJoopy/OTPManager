using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OTPManager.Models;
using OTPManager.Services;
using System.Net.Mail;


namespace OTPManager.Filters
{

    public class SendEmailTotp : IAsyncActionFilter
    {


        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next(); // Ensure action execution completes

            if (executedContext.Result is OkObjectResult)
            {
                Dictionary<string, string> details = (Dictionary<string, string>)context.HttpContext.Items["userDetails"];

                var httpContext = context.HttpContext;
                var logger = httpContext.RequestServices.GetRequiredService<ILogger<SendSMSTotp>>();
                httpContext.Items.TryGetValue("SEND_EMAIL", out var shouldSendEmailObj);
                bool.TryParse((shouldSendEmailObj ?? "false").ToString(), out var shouldSendEmail);

                if (shouldSendEmail)
                {
                    var emailService = context.HttpContext.RequestServices.GetRequiredService<EmailService>();

                    // Define your email parameters
                    string to = details["EMAIL"] ?? "shir.l@incentives-solutions.com";
                    string[] cc = new string[] { };
                    string subject = "One Time Code";
                    string body = $@"<p>Your one time code is {context.HttpContext.Items["TOTP"]}</p>";

                    await emailService.SendEmailAsync(to, subject, body);
                }
             }

            // Before the action executes


        }
    }
}

