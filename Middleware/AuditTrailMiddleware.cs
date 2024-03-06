
    public class AuditTrailMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditTrailMiddleware> _logger;

        public AuditTrailMiddleware(RequestDelegate next, ILogger<AuditTrailMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Log the request information
            _logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");

            // You might want to capture the request body here
            // Be mindful of performance and security implications

            // Await the response
            await _next(context);

            // Optionally, log response information
            // Note: Accessing the response body is more involved and requires buffering the response
        }
    }

