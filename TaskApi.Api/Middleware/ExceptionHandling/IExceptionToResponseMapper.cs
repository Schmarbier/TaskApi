using TaskApi.Api.Middleware.ExceptionHandling.Models;

namespace TaskApi.Api.Middleware.ExceptionHandling
{
    public interface IExceptionToResponseMapper
    {
        ErrorResponse Map(Exception exception, HttpContext context);
    }
}
