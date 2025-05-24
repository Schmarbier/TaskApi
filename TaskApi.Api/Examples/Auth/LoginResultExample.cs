using Swashbuckle.AspNetCore.Filters;
using TaskApi.Application.Features.Auth.DTOs;

namespace TaskApi.Api.Examples.Auth
{
    public class LoginResultExample : IExamplesProvider<LoginResult>
    {
        public LoginResult GetExamples()
        {
            return new LoginResult
            {
                UserId = Guid.NewGuid(),
                AccessToken = "esu29sj2nsASDj29sASDja982mnasSDp...",
                RefreshToken = "Sdm92msDdja92PTLKsdv27HU..."
            };
        }
    }
}
