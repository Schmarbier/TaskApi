using TaskApi.Domain.Entities;

namespace TaskApi.Application.Interfaces
{
    public interface IAuthService
    {
        string GenerarToken(User usuario);
        string GenerarRefreshToken();
    }
}
