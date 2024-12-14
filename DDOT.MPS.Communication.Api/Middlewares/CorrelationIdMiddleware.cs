namespace DDOT.MPS.Communication.Api.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "x-correlation-id";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request has a correlation ID
            if (!context.Request.Headers.ContainsKey(CorrelationIdHeader))
            {
                var correlationId = Guid.NewGuid().ToString();

                // Add the new correlation ID to the request headers
                context.Request.Headers[CorrelationIdHeader] = correlationId;

                // Add the correlation ID to the response headers
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[CorrelationIdHeader] = correlationId;
                    return Task.CompletedTask;
                });
            }
            else
            {
                // Propagate the existing correlation ID to the response headers
                var correlationId = context.Request.Headers[CorrelationIdHeader].ToString();
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[CorrelationIdHeader] = correlationId;
                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }
    }
}
