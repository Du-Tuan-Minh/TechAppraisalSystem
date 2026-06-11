using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TechAppraisalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController<T> : ControllerBase where T : class
    {
        protected readonly IServiceManager _serviceManager;
        protected readonly ILogger<T> _logger;
        protected BaseApiController(IServiceManager serviceManager, ILogger<T> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;
        }
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var guid) ? guid : Guid.Empty;
        }

        protected IEnumerable<string> CurrentUserRoles => User.FindAll(ClaimTypes.Role).Select(r => r.Value);
        protected string? CurrentUserName => User.Identity?.Name;

        protected string? GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role);
        }

        protected bool IsAdmin()
        {
            return User.IsInRole(nameof(UserRole.Admin));
        }
    }
}