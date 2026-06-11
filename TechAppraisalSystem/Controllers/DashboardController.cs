using Application.Common;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : BaseApiController<DashboardController>
    {
        public DashboardController(IServiceManager services, ILogger<DashboardController> logger) : base(services, logger) { }

        [Authorize(Roles = nameof(UserRole.Staff))]
        [HttpGet("staff-summary")]
        public async Task<IActionResult> GetStaffSummary()
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DashboardService.GetStaffDashboardSummaryAsync(userId);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Manager))]
        [HttpGet("manager-summary")]
        public async Task<IActionResult> GetManagerSummary()
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DashboardService.GetManagerDashboardSummaryAsync(userId);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("manager-requested-document-summary")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> GetDepartmentDocumentStatusSummary()
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DashboardService.GetDepartmentDocumentStatusSummaryAsync(userId);
            return Ok(result);
        }


        [HttpGet("director-summary")]
        [Authorize(Roles = nameof(UserRole.Director))]
        public async Task<IActionResult> GetDirectorSummary()
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DashboardService.GetDirectorDashboardSummaryAsync(userId);

            return Ok(result);
        }

        [HttpGet("manager-workloads")]
        [Authorize(Roles = nameof(UserRole.Director))]
        public async Task<IActionResult> GetManagerWorkloads([FromQuery] PaginationDto pagination, [FromQuery] string? searchTerm)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DashboardService.GetManagerWorkloadsAsync(userId, pagination, searchTerm);

            return Ok(result);
        }

        [HttpGet("coordinator-summary")]
        [Authorize(Roles = nameof(UserRole.Coordinator))]
        public async Task<IActionResult> GetCoordinatorSummary()
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DashboardService.GetCoordinatorDashboardSummaryAsync(userId);
            return Ok(result);
        }
    }
}