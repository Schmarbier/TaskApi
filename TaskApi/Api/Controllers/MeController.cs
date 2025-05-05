using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskApi.Api.Responses;

namespace TaskApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly ILogger<MeController> _logger;

        public MeController(ILogger<MeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUserInfo()
        {
            _logger.LogInformation($"Usuario obtiene su informacion");
            var email = User.Identity?.Name;
            return Ok(ApiResponse.Ok(new { email }));
        }
    }
}
