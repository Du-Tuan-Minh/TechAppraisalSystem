using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseApiController<AuthController>
    {
        public AuthController(IServiceManager services, ILogger<AuthController> logger)
         : base(services, logger)
        { }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _serviceManager.AuthService.RegisterAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _serviceManager.AuthService.LoginAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var result = await _serviceManager.AuthService.RefreshTokenAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            var result = await _serviceManager.AuthService.LogoutAsync(request.RefreshToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}