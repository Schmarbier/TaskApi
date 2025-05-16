using System.Text.Json;
using TaskApi.Application.Common.Exceptions;
using TaskApi.Domain.Exceptions;

namespace TaskApi.Api.Middleware.ExceptionHandling
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.TraceIdentifier;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var mapper = context.RequestServices.GetRequiredService<IExceptionToResponseMapper>();
                var response = mapper.Map(ex, context);

                context.Response.StatusCode = response.StatusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(response.Body));
            }
        }
    }
}
