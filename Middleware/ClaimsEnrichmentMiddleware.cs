using Microsoft.AspNetCore.Http;
using OTPManager.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Vonage.Users;


namespace OTPManager.Middleware
{

    public class ClaimsEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        public ClaimsEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IVerificationService verificationService)
        {
            // Check if user is authenticated and has claims
            if (context.User.Identity.IsAuthenticated)
            {
                var tenantId = context.User.FindFirst("tenantId")?.Value;
                var userId = context.User.FindFirst("userId")?.Value;
                var username = context.User.FindFirst("userName")?.Value;
                var identifier = context.User.FindFirst("identifier")?.Value;
                var userDetails = verificationService.GetUser(Int32.Parse(userId));



                // Add tenantId and userId to the HttpContext.Items collection for easy access
                if (tenantId != null)
                {
                    context.Items["tenantId"] = tenantId;
                }

                if (userId != null)
                {
                    context.Items["userId"] = userId;
                }

                if (username != null)
                {
                    context.Items["username"] = username;
                }

                if (identifier != null)
                {
                    context.Items["identifier"] = identifier;
                }

                context.Items["userDetails"] = userDetails;
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
