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

        public async Task InvokeAsync(HttpContext context)
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

                    var elapsed = stopwatch.ElapsedMilliseconds;
                    var statusCode = context.Response.StatusCode;
                    var level = statusCode >= 500 ? LogLevel.Error :
                                statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

                    var log = $"HTTP {context.Request.Method} {context.Request.Path} ded {statusCode} in {elapsed} ms";

                    _logger.Log(level, log);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, $"HTTP {context.Request.Method} {context.Request.Path} failed in {stopwatch.ElapsedMilliseconds} ms");
                    throw;
                }
            }
        }
    }
}
