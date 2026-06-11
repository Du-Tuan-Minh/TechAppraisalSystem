using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/approval-workflows")]
    public class ApprovalWorkflowController : BaseApiController<ApprovalWorkflowController>
    {
        public ApprovalWorkflowController(IServiceManager services, ILogger<ApprovalWorkflowController> logger)
            : base(services, logger) { }

        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> GetDocumentWorkflow(Guid documentId, [FromQuery] Guid? requestVersionId)
        {
            var result = await _serviceManager.ApprovalWorkflowService.GetDocumentWorkflowAsync(documentId, requestVersionId);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkflowDetail(Guid id)
        {
            var result = await _serviceManager.ApprovalWorkflowService.GetWorkflowDetailAsync(id);

            return StatusCode(result.StatusCode, result);
        }
    }
}