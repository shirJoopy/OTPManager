using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OTPManager.Middleware { 
    public class JsonResponse
    {
        private readonly RequestDelegate _next;

        public JsonResponse(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey("Content-Type"))
                {
                    context.Response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                }
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
