using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAppraisalHistoryService
    {
        Task<ApiResponse<PagedResult<AppraisalHistoryResponseDto>>> GetDocumentAuditTrailAsync(Guid documentId, PaginationDto pagination);
        Task<ApiResponse<AppraisalHistoryResponseDto>> GetHistoryByVersionAsync(Guid versionId);
    }
}