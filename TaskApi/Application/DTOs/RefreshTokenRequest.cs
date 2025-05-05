using System.ComponentModel.DataAnnotations;

namespace TaskApi.Application.DTOs
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "El token es obligatorio.")]
        public string Token { get; set; } = string.Empty;
        [Required(ErrorMessage = "El refresh token es obligatorio.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
