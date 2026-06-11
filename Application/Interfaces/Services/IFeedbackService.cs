using Application.Common;
using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IFeedbackService
    {
        Task<ApiResponse<PagedResult<FeedbackIssueResponseDto>>> GetIssuesByDocumentIdAsync(Guid documentId, PaginationDto pagination);
        Task<ApiResponse<PagedResult<FeedbackIssueResponseDto>>> GetIssuesByVersionIdAsync(Guid versionId, PaginationDto pagination);
        Task<ApiResponse<FeedbackIssueDetailDto>> GetIssueByIdAsync(Guid issueId);
        Task<ApiResponse<bool>> UpdateIssueStatusAsync(Guid issueId, IssueStatus newStatus, string? note, IssueCategory? issueType, Guid staffId);
        Task<ApiResponse<bool>> FinalizeIssueClosureAsync(Guid issueId, Guid specialistId);
        Task<ApiResponse<List<FeedbackIssueResponseDto>>> AddReviewIssuesAsync(List<FeedbackIssueCreateDto> dtos, Guid staffId);
    }
}