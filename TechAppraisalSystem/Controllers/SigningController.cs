using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/signing")]
    public class SigningController : BaseApiController<SigningController>
    {
        public SigningController(IServiceManager services, ILogger<SigningController> logger)
      : base(services, logger)
        { }

        //[HttpGet("{documentId}/progress")]
        //public async Task<IActionResult> GetProgress(Guid documentId, [FromQuery] Guid versionId)
        //{
        //    var result = await _serviceManager.WorkflowService.GetWorkflowProgressAsync(documentId, versionId);
        //    return StatusCode(result.StatusCode, result);
        //}

        [HttpPost("approve-sign")]
        [Authorize(Roles = nameof(UserRole.Director))]
        public async Task<IActionResult> Approve([FromBody] SendParallelAssignmentsRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.SigningService.SignDocumentAsync(request, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("reject")]
        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Director)}")]
        public async Task<IActionResult> Reject([FromBody] AppraisalRejectDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.SigningService.RejectDocumentAsync(request, userId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.InstituteDirector))]
        [HttpPost("{docId}/issue")]
        public async Task<IActionResult> PublishDocument(Guid docId)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.SigningService.IssueDocumentAsync(docId, userId);
            return StatusCode(result.StatusCode, result);
        }

        //[Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Director)}, {nameof(UserRole.Inspector)}")]
        //[HttpGet("export-pdf/{versionId}")]
        //public async Task<IActionResult> GetExportfByVersion(Guid versionId)
        //{
        //    var result = await _serviceManager.SigningService.ExportVersionPdfAsync(versionId);
        //    if (!result.IsSuccess) return StatusCode(result.StatusCode, result);
        //    var fileData = result.Data;

        //    return File(fileData.FileStream, "application/pdf", $"{fileData.FileName}_.pdf");
        //}
    }
}