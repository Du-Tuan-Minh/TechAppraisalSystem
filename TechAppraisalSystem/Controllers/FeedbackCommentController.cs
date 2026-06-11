using Application.Common;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [ApiController]
    [Route("api/feedback-comments")]
    [Authorize]
    public class FeedbackCommentController : BaseApiController<FeedbackCommentController>
    {
        public FeedbackCommentController(IServiceManager services, ILogger<FeedbackCommentController> logger)
        : base(services, logger)
        { }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateFeedbackCommentRequest request)
        {
            var userId = GetCurrentUserId();

            var result = await _serviceManager.FeedbackCommentService.CreateCommentAsync(userId, request);
            return Ok(result);
        }

        [HttpGet("issue/{issueId}")]
        public async Task<IActionResult> GetByIssue(Guid issueId, [FromQuery] PaginationDto pagination)
        {
            var results = await _serviceManager.FeedbackCommentService.GetCommentsByIssueAsync(issueId, pagination);
            return Ok(results);
        }
    }
}