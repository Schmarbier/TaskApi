using System.Net;
using System.Text.Json;
using TaskApi.Application.Responses;

namespace TaskApi.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid().ToString();

                _logger.LogError(ex, "Error inesperado, ErrorId: {ErrorId}", errorId);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = ApiResponse.Fail(500,"Ocurrió un error interno. Si el problema persiste, contactá al soporte.", errorId);

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
