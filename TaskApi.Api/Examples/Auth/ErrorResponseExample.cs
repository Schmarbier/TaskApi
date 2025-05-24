using Swashbuckle.AspNetCore.Filters;
using TaskApi.Api.Middleware.ExceptionHandling.Models;

namespace TaskApi.Api.Examples.Auth
{
    public class ErrorResponseExample : IExamplesProvider<ErrorResponse>
    {
        public ErrorResponse GetExamples()
        {
            return new ErrorResponse(400, new
            {
                message = "Se encontraron errores de validación",
                errors = new[]
                {
                new { PropertyName = "Email", ErrorMessage = "El email es obligatorio." },
                new { PropertyName = "Password", ErrorMessage = "La contraseña debe tener al menos 6 caracteres." }
            }
            });
        }
    }
}
