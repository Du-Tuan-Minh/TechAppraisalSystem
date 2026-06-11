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
    [Route("api/appraisal")]
    public class AppraisalController : BaseApiController<AppraisalController>
    {
        public AppraisalController(IServiceManager services, ILogger<AppraisalController> logger)
            : base(services, logger)
        { }

        [Authorize(Roles = $"{nameof(UserRole.Director)}, {nameof(UserRole.Coordinator)}, {nameof(UserRole.InstituteDirector)}, {nameof(UserRole.DeputyInstituteDirector)}")]
        [HttpPost("create-parallel-assignments")]
        public async Task<IActionResult> CreateParallelAssignments([FromBody] CreateParallelAssignmentsRequest request)
        {
            var senderId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.CreateParallelAssignmentsAsync(request, senderId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Manager))]
        [HttpPost("assign-internal-staff")]
        public async Task<IActionResult> AssignStaff([FromBody] AssignStaffRequest request)
        {
            var managerId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.AssignInternalStaffAsync(request, managerId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.DeputyInstituteDirector)}")]
        [HttpPost("department-confirm")]
        public async Task<IActionResult> ConfirmDepartment([FromBody] CompleteAssignmentRequest request, [FromQuery] Guid documentId)
        {
            var managerId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.ConfirmDepartmentResultAsync(documentId, request, managerId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Staff))]
        [HttpPut("staff-submit-review/{reviewerId}")]
        public async Task<IActionResult> SubmitReview(Guid reviewerId, [FromBody] UpdateReviewerProgressRequest request)
        {
            var staffId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.SubmitStaffReviewAsync(reviewerId, request, staffId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Director)}, {nameof(UserRole.InstituteDirector)}")]
        [HttpPost("center-confirm")]
        public async Task<IActionResult> ConfirmCenter(ConsolidateAppraisalRequest request)
        {
            var managerId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.ConfirmCenterResultAsync(request, managerId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("assignment-detail/{id}")]
        public async Task<IActionResult> GetDetailById(Guid id)
        {
            var result = await _serviceManager.AppraisalService.GetAssignmentDetailAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Director)}, {nameof(UserRole.InstituteDirector)}, {nameof(UserRole.DeputyInstituteDirector)}")]
        [HttpGet("director-assignments")]
        public async Task<IActionResult> GetListAssignments([FromQuery] PaginationDto pagination)
        {
            var directorId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.GetAssignmentsForDirectorAsync(directorId, pagination);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Director)}, {nameof(UserRole.InstituteDirector)}, {nameof(UserRole.DeputyInstituteDirector)}")]
        [HttpGet("manager-assignments/{versionId?}")]
        public async Task<IActionResult> GetManagerAssignments([FromRoute] Guid? versionId, [FromQuery] PaginationDto pagination)
        {
            var managerId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.GetAssignmentsForManagerAsync(managerId, versionId, pagination);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Staff)}")]
        [HttpGet("reviewer-detail/{reviewerId}")]
        public async Task<IActionResult> GetReviewerDetail(Guid reviewerId)
        {
            var result = await _serviceManager.AppraisalService.GetReviewerDetailAsync(reviewerId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Staff)}")]
        [HttpGet("list-reviewer/{assignmentId?}")]
        public async Task<IActionResult> GetListReviewer(Guid? assignmentId, [FromQuery] PaginationDto pagination)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.GetAssignmentsReviewForStaffAsync(assignmentId, userId, pagination);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Director)}, {nameof(UserRole.InstituteDirector)}")]
        [HttpPost("recall-assignments")]
        public async Task<IActionResult> ForceCompleteAssignments([FromBody] Guid documentId)
        {
            var directorId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.RecallAssignmentsAsync(documentId, directorId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Coordinator))]
        [HttpPost("coordinator-assign")]
        public async Task<IActionResult> CoordinatorAssign([FromBody] CoordinatorAssignRequest request)
        {
            var coordinatorId = GetCurrentUserId();
            var result = await _serviceManager.AppraisalService.CoordinatorAssignAsync(request, coordinatorId);

            return StatusCode(result.StatusCode, result);
        }
    }
}