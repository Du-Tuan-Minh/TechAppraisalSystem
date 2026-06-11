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
    [Route("api/feedback")]
    public class FeedbackController : BaseApiController<FeedbackController>
    {
        public FeedbackController(IServiceManager services, ILogger<FeedbackController> logger)
        : base(services, logger)
        { }

        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> GetIssuesByDocument(Guid documentId, [FromQuery] PaginationDto pagination)
        {
            var result = await _serviceManager.FeedbackService.GetIssuesByDocumentIdAsync(documentId, pagination);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("version/{versionId}")]
        public async Task<IActionResult> GetIssuesByVersion(Guid versionId, [FromQuery] PaginationDto pagination)
        {
            var result = await _serviceManager.FeedbackService.GetIssuesByVersionIdAsync(versionId, pagination);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetIssueDetail(Guid id)
        {
            var result = await _serviceManager.FeedbackService.GetIssueByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Staff)},{nameof(UserRole.Manager)},{nameof(UserRole.Director)},{nameof(UserRole.Inspector)},{nameof(UserRole.InstituteDirector)},{nameof(UserRole.DeputyInstituteDirector)}")]
        [HttpPost("add-issues")]
        public async Task<IActionResult> AddReviewIssues([FromBody] List<FeedbackIssueCreateDto> requests)
        {
            var staffId = GetCurrentUserId();
            var result = await _serviceManager.FeedbackService.AddReviewIssuesAsync(requests, staffId);
            return StatusCode(result.StatusCode, result);
        }

        //[Authorize(Roles = nameof(UserRole.Staff))]
        //[HttpPatch("issues/{id}/status")]
        //public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] FeedbackIssueUpdateDto request)
        //{
        //    var staffId = GetCurrentUserId();
        //    var result = await _serviceManager.FeedbackService.UpdateIssueStatusAsync(id, request.Status, request.ResolutionNote, request.Severity, staffId);
        //    return StatusCode(result.StatusCode, result);
        //}

        //[Authorize(Roles = nameof(UserRole.Manager))]
        //[HttpPost("issues/{id}/finalize")]
        //public async Task<IActionResult> FinalizeClosure(Guid id)
        //{
        //    var specialistId = GetCurrentUserId();
        //    var result = await _serviceManager.FeedbackService.FinalizeIssueClosureAsync(id, specialistId);
        //    return StatusCode(result.StatusCode, result);
        //}
    }
}