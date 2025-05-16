
using TaskApi.Api.Middleware.ExceptionHandling.Models;
using TaskApi.Application.Common.Exceptions;
using TaskApi.Domain.Exceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace TaskApi.Api.Middleware.ExceptionHandling
{
    public class ExceptionToResponseMapper : IExceptionToResponseMapper
    {
        public ErrorResponse Map(Exception exception, HttpContext context) 
        {
            var correlationId = context.TraceIdentifier;

            Console.WriteLine($"Excepción recibida: {exception.GetType()}");

            return exception switch
            {
                ValidationException validationException
                => new ErrorResponse(400, new
                {
                    message = "Se encontraron errores de validación",
                    errors = validationException.Errors
                }),

                UnauthorizedAccessException unauthorizedAccessException
                => new ErrorResponse(401, new
                {
                    message = unauthorizedAccessException.Message
                }),

                BusinessRuleException businessRuleException
                => new ErrorResponse(businessRuleException.InternalCode, new
                {
                    message = businessRuleException.Message
                }),

                _ => new ErrorResponse(500, new
                {
                    message = "Ocurrió un error inesperado. Si el error persiste, comuníquese con el administrador.",
                    correlationId
                })
            };
        }
    }
}
