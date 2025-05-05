using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskApi.Infraestructure.Data;

namespace TaskApi.Infraestructure.Security
{
    public class JwtBearerEventsHandler : JwtBearerEvents
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public JwtBearerEventsHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            OnTokenValidated = async context =>
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    context.Fail("Token inválido");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                var user = await db.Usuarios.FindAsync(int.Parse(userId));

                var handler = new JwtSecurityTokenHandler();

                var rawToken = context.HttpContext.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                if (user == null || user.Token != rawToken)
                {
                    context.Fail("Token inválido o revocado");
                }
            };
        }
    }
}
