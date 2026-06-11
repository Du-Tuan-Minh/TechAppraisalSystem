using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface ISigningService
    {
        Task<ApiResponse<bool>> SignDocumentAsync(SendParallelAssignmentsRequest request, Guid userId);
        Task<ApiResponse<bool>> RejectDocumentAsync(AppraisalRejectDto request, Guid userId);
        Task<ApiResponse<bool>> IssueDocumentAsync(Guid documentId, Guid userId);
        //Task<ApiResponse<FileStreamResultDto>> ExportVersionPdfAsync(Guid versionId);
    }
}