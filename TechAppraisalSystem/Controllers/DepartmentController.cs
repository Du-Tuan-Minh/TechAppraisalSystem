using Application.Common;
using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/departments")]
    public class DepartmentController : BaseApiController<DepartmentController>
    {
        public DepartmentController(IServiceManager services, ILogger<DepartmentController> logger)
        : base(services, logger)
        { }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Director)}, {nameof(UserRole.InstituteDirector)}")]
        [HttpPost("invite")]
        public async Task<IActionResult> Invite([FromBody] DepartmentInvitationCreateDto dto)
        {
            var managerId = GetCurrentUserId();
            var result = await _serviceManager.DepartmentService.InviteToDepartmentAsync(managerId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Director)}, {nameof(UserRole.InstituteDirector)}")]
        [HttpPost("create-department")]
        public async Task<IActionResult> Create([FromBody] DepartmentCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DepartmentService.CreateDepartmentAsync(dto, userId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Director)}, {nameof(UserRole.InstituteDirector)}")]
        [HttpPut("{id}/update-department")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DepartmentUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DepartmentService.UpdateDepartmentAsync(id, userId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Director)}, {nameof(UserRole.InstituteDirector)}")]
        [HttpDelete("{id}/delete-department")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DepartmentService.DeleteDepartmentAsync(id, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("centers")]
        public async Task<IActionResult> GetCenters([FromQuery] PaginationDto pagination, [FromQuery] string? searchTerm)
        {
            var result = await _serviceManager.DepartmentService.GetCentersAsync(pagination, searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{parentId:guid}/sub-departments")]
        public async Task<IActionResult> GetSubDepartments(Guid parentId, [FromQuery] PaginationDto pagination, [FromQuery] string? searchTerm)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DepartmentService.GetSubDepartmentsAsync(userId, parentId, pagination, searchTerm);
            return StatusCode(result.StatusCode, result);
        }
    }
}