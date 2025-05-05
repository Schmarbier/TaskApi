using Serilog.Context;
using System.Security.Claims;

namespace TaskApi.Middleware.Log
{
    public class LogUserIdMiddleware
    {
        private readonly RequestDelegate _next;

        public LogUserIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                using (LogContext.PushProperty("UserId", userId))
                {
                    await _next(context); // Todos los logs de este request heredarán esa propiedad
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
