using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IApprovalWorkflowService
    {
        Task<ApiResponse<List<ApprovalWorkflowResponseDto>>> GetDocumentWorkflowAsync(Guid documentId, Guid? requestVersionId);
        Task<ApiResponse<ApprovalWorkflowDetailDto>> GetWorkflowDetailAsync(Guid id);
    }
}