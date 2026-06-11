using Application.Common;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/appraisal-history")]
    public class AppraisalHistoryController : BaseApiController<AppraisalHistoryController>
    {
        public AppraisalHistoryController(IServiceManager services, ILogger<AppraisalHistoryController> logger)
          : base(services, logger)
        { }

        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> GetAuditTrail(Guid documentId, [FromQuery] PaginationDto pagination)
        {
            var result = await _serviceManager.AppraisalHistoryService.GetDocumentAuditTrailAsync(documentId, pagination);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("version/{versionId}")]
        public async Task<IActionResult> GetHistoryByVersion(Guid versionId)
        {
            var result = await _serviceManager.AppraisalHistoryService.GetHistoryByVersionAsync(versionId);
            return StatusCode(result.StatusCode, result);
        }
    }
}