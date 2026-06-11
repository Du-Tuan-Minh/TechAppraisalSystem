using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IFeedbackCommentService
    {
        Task<ApiResponse<FeedbackCommentDto>> CreateCommentAsync(Guid userId, CreateFeedbackCommentRequest request);
        Task<ApiResponse<PagedResult<FeedbackCommentDto>>> GetCommentsByIssueAsync(Guid issueId, PaginationDto pagination);
    }
}