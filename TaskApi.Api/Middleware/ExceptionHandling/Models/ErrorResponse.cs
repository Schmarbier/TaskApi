namespace TaskApi.Api.Middleware.ExceptionHandling.Models
{
    public record ErrorResponse(int StatusCode, object Body);
}
