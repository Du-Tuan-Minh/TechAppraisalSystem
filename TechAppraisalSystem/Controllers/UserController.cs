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
    [Route("api/users")]
    public class UserController : BaseApiController<UserController>
    {
        public UserController(IServiceManager services, ILogger<UserController> logger) : base(services, logger) { }

        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.UserService.GetMyProfileAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.UserService.UpdateProfileAsync(userId, request);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Staff)}, {nameof(UserRole.DeputyInstituteDirector)}")]
        [HttpPost("join-department")]
        public async Task<IActionResult> JoinDepartment([FromQuery] string inviteCode)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.DepartmentService.JoinDepartmentAsync(userId, inviteCode);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("request-promotion")]
        public async Task<IActionResult> RequestPromotion([FromBody] string reason, [FromQuery] UserRole requestedRole)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.UserService.RequestRolePromotionAsync(userId, requestedRole, reason);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPatch("{id}/account-status")]
        public async Task<IActionResult> UpdateAccountStatus(Guid id, [FromBody] UserUpdateAccountDto request)
        {
            var result = await _serviceManager.UserService.UpdateUserAccountAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user-filter")]
        public async Task<IActionResult> GetUserList([FromQuery] UserFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.UserService.GetUsersPagedAsync(filter, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.UserService.ChangePasswordAsync(userId, request);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)}, {nameof(UserRole.Director)}, {nameof(UserRole.Manager)}, {nameof(UserRole.DeputyInstituteDirector)}, {nameof(UserRole.InstituteDirector)}")]
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetUserDetail(Guid id)
        {
            var result = await _serviceManager.UserService.GetUserDetailAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("senior-center")]
        public async Task<IActionResult> GetSeniorCenter([FromQuery] PaginationDto pagination, [FromQuery] string? searchTerm)
        {
            var result = await _serviceManager.UserService.GetSeniorCenterAsync(pagination, searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        //[Authorize(Roles = nameof(UserRole.Manager))]
        //[HttpGet("top-document-authors")]
        //public async Task<IActionResult> GetTopDocumentAuthors([FromQuery] PaginationDto pagination, [FromQuery] string? searchTerm)
        //{
        //    var userId = GetCurrentUserId();
        //    var result = await _serviceManager.UserService.GetTopDocumentAuthorsAsync(userId, pagination, searchTerm);

        //    return StatusCode(result.StatusCode, result);
        //}

        [Authorize(Roles = nameof(UserRole.Manager))]
        [HttpGet("top-rejected-document-types")]
        public async Task<IActionResult> GetTopRejectedDocuments([FromQuery] PaginationDto pagination)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.UserService.GetTopRejectedDocumentsAsync(userId, pagination);

            return StatusCode(result.StatusCode, result);
        }


        [HttpGet("department-appraisal-workloads")]
        [Authorize(Roles = nameof(UserRole.Coordinator))]
        public async Task<IActionResult> GetDepartmentAppraisalWorkloads([FromQuery] PaginationDto pagination, [FromQuery] string? searchTerm)
        {
            var result = await _serviceManager.UserService.GetDepartmentAppraisalWorkloadsAsync(pagination, searchTerm);
            return Ok(result);
        }
    }
}