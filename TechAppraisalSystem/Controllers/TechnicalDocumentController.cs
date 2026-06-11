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
    [Route("api/documents")]
    public class TechnicalDocumentController : BaseApiController<TechnicalDocumentController>
    {
        public TechnicalDocumentController(IServiceManager services, ILogger<TechnicalDocumentController> logger)
      : base(services, logger)
        { }

        [Authorize(Roles = nameof(UserRole.Staff))]
        [HttpPost("create-document")]
        public async Task<IActionResult> CreateDocument([FromBody] TechnicalDocumentCreateDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.CreateDocumentAsync(request, userId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = $"{nameof(UserRole.Manager)}, {nameof(UserRole.Staff)}")]
        [HttpPost("{id}/submit-internal")]
        public async Task<IActionResult> SubmitInternal(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.SubmitInternalApprovalAsync(id, userId);
            return StatusCode(result.StatusCode, result);
        }

        //[Authorize(Roles = nameof(UserRole.Staff))]
        //[HttpPost("{id}/create-version")]
        //public async Task<IActionResult> HandleFeedback(Guid id, [FromBody] DocumentVersionCreateDto request)
        //{
        //    var userId = GetCurrentUserId();
        //    var result = await _serviceManager.TechnicalDocumentService.CreateVersionAsync(id, userId, request);
        //    return StatusCode(result.StatusCode, result);
        //}

        //[Authorize(Roles = nameof(UserRole.Staff))]
        //[HttpPost("create-version-issue/{issueId}")]
        //public async Task<IActionResult> CreateFromImprovement(Guid issueId, [FromBody] DocumentVersionCreateDto dto)
        //{
        //    var userId = GetCurrentUserId();
        //    var result = await _serviceManager.TechnicalDocumentService.CreateImprovementVersionAsync(issueId, userId, dto);
        //    return StatusCode(result.StatusCode, result);
        //}

        [HttpGet("{id}/document-detail")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await _serviceManager.TechnicalDocumentService.GetDocumentDetailAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Staff))]
        [HttpPut("{id}/update-draft")]
        public async Task<IActionResult> UpdateDraft(Guid id, [FromBody] TechnicalDocumentUpdateDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.UpdateDraftAsync(id, userId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("list-document")]
        public async Task<IActionResult> GetDocuments([FromQuery] DocumentFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetDocumentsPagedAsync(filter, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{documentId}/versions")]
        public async Task<IActionResult> GetVersions(Guid documentId)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetDocumentVersionsAsync(documentId, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasks([FromQuery] PaginationDto pagination)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetMyTasksPagedAsync(userId, pagination);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("versions/{versionId}/detail")]
        public async Task<IActionResult> GetVersionDetail(Guid versionId)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetVersionDetailAsync(versionId, userId);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Staff))]
        [HttpGet("my-current-documents")]
        public async Task<IActionResult> GetMyCurrentDocuments([FromQuery] UserCurrentDocumentFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetMyCurrentDocumentsAsync(userId, filter);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Manager))]
        [HttpGet("manager-status-documents")]
        public async Task<IActionResult> GetManagerStatusDocuments([FromQuery] ManagerDashboardDocumentFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetManagerStatusDocumentsAsync(userId, filter);

            return StatusCode(result.StatusCode, result);
        }

        //[Authorize(Roles = nameof(UserRole.Manager))]
        //[HttpGet("pending-appraisal-responses")]
        //public async Task<IActionResult> GetPendingAppraisalResponses([FromQuery] PendingAppraisalFilterDto filter)
        //{
        //    var userId = GetCurrentUserId();
        //    var result = await _serviceManager.TechnicalDocumentService.GetPendingAppraisalResponsesAsync(userId, filter);

        //    return StatusCode(result.StatusCode, result);
        //}

        [Authorize(Roles = nameof(UserRole.Manager))]
        [HttpGet("manager-overdue-documents")]
        public async Task<IActionResult> GetOverdueDocuments([FromQuery] OverdueFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetOverdueDocumentsAsync(userId, filter);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Manager))]
        [HttpGet("manager-requested-overdue-documents")]
        public async Task<IActionResult> GetDepartmentCreatedOverdueDocuments([FromQuery] OverdueFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetManagerRequestOverdueDocumentsAsync(userId, filter);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("manager-request-status-documents")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> GetManagerRequestStatusDocuments([FromQuery] DepartmentDocumentStatusFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetManagerRequestStatusDocumentsAsync(userId, filter);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = nameof(UserRole.Coordinator))]
        [HttpGet("incoming-appraisal-documents")]
        public async Task<IActionResult> GetIncomingAppraisalDocuments([FromQuery] PaginationDto pagination, [FromQuery] string? searchTerm)
        {
            var coordinatorId = GetCurrentUserId();
            var result = await _serviceManager.TechnicalDocumentService.GetIncomingAppraisalDocumentsAsync(coordinatorId, pagination, searchTerm);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user-appraisal-workloads/{userId:guid}/documents")]
        [Authorize(Roles = nameof(UserRole.Coordinator))]
        public async Task<IActionResult> GetUserWorkloadDocuments(Guid userId, [FromQuery] PaginationDto pagination, [FromQuery] string? searchTerm)
        {
            var result = await _serviceManager.TechnicalDocumentService.GetUserWorkloadDocumentsAsync(userId, pagination, searchTerm);
            return Ok(result);
        }
    }
}