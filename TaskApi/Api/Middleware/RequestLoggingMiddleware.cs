using Azure;
using Serilog.Context;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskApi.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var request = context.Request;
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = request.Headers["User-Agent"].FirstOrDefault();
            var method = request.Method;
            var path = request.Path;

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var correlationId = context.TraceIdentifier;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
            using (LogContext.PushProperty("ClientIp", ip ?? "unknown"))
            using (LogContext.PushProperty("UserAgent", userAgent ?? "unknown"))
            using (LogContext.PushProperty("RequestMethod", method))
            using (LogContext.PushProperty("RequestPath", path))
            {
                try
                {
                    await _next(context);

                    stopwatch.Stop();
                    var statusCode = context.Response.StatusCode;

                    using (LogContext.PushProperty("StatusCode", context.Response.StatusCode))
                    using (LogContext.PushProperty("RequestDurationMs", stopwatch.ElapsedMilliseconds))
                    {
                        _logger.LogInformation("Request: {Method} {Path} - {StatusCode} en {Elapsed}ms", method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
                    }

                }
                catch (Exception ex)
                {
                    using (LogContext.PushProperty("StatusCode", 500))
                    using (LogContext.PushProperty("RequestDurationMs", stopwatch.ElapsedMilliseconds))
                    {
                        _logger.LogError(ex, "Error no controlado (ErrorId: {ErrorId}) en {Method} {Path}", correlationId, method, path);

                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            message = "Ocurrió un error interno. Si el problema persiste, contactá al soporte.",
                            errorId = correlationId
                        };

                        await context.Response.WriteAsJsonAsync(response);
                    }
                }


            }
        }
    }
}
