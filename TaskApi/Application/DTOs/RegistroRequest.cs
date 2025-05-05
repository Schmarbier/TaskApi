namespace TaskApi.Application.DTOs
{
    public class RegistroRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
