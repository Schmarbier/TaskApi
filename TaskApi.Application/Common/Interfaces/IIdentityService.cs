namespace TaskApi.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<(bool Success, Guid UserId)> CreateUserAsync(string username, string email, string password, string role);
        Task<(bool Success, Guid UserId, string Token, string RefreshToken)> LoginAsync(string email, string password);
        Task<(bool Success, Guid UserId, string Token, string RefreshToken)> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
