using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface ITechnicalDocumentService
    {
        Task<ApiResponse<DocumentVersionDetailDto>> GetVersionDetailAsync(Guid versionId, Guid userId);
        Task<ApiResponse<TechnicalDocumentDetailDto>> GetDocumentDetailAsync(Guid id);
        Task<ApiResponse<bool>> SubmitInternalApprovalAsync(Guid documentId, Guid requesterId);
        //Task<ApiResponse<DocumentVersionDto>> CreateVersionAsync(Guid documentId, Guid userId, DocumentVersionCreateDto dto);
        Task<ApiResponse<TechnicalDocumentDetailDto>> CreateDocumentAsync(TechnicalDocumentCreateDto request, Guid requesterId);
        // Task<ApiResponse<DocumentVersionDto>> CreateImprovementVersionAsync(Guid issueId, Guid userId, DocumentVersionCreateDto dto);
        Task<ApiResponse<bool>> UpdateDraftAsync(Guid documentId, Guid userId, TechnicalDocumentUpdateDto dto);
        Task<ApiResponse<PagedResult<TechnicalDocumentResponseDto>>> GetDocumentsPagedAsync(DocumentFilterDto filter, Guid userId);
        Task<ApiResponse<IEnumerable<DocumentVersionDto>>> GetDocumentVersionsAsync(Guid documentId, Guid userId);
        Task<ApiResponse<PagedResult<TechnicalDocumentResponseDto>>> GetMyTasksPagedAsync(Guid userId, PaginationDto pagination);
        Task<ApiResponse<PagedResult<UserCurrentDocumentDto>>> GetMyCurrentDocumentsAsync(Guid userId, UserCurrentDocumentFilterDto filter);
        //Task<ApiResponse<PagedResult<PendingAppraisalResponseDto>>> GetPendingAppraisalResponsesAsync(Guid managerId, PendingAppraisalFilterDto filter);
        Task<ApiResponse<PagedResult<OverdueDocumentDto>>> GetOverdueDocumentsAsync(Guid managerId, OverdueFilterDto filter);
        Task<ApiResponse<PagedResult<ManagerDashboardDocumentDto>>> GetManagerStatusDocumentsAsync(Guid managerId, ManagerDashboardDocumentFilterDto filter);
        Task<ApiResponse<PagedResult<OverdueDocumentDto>>> GetManagerRequestOverdueDocumentsAsync(Guid directorId, OverdueFilterDto filter);
        Task<ApiResponse<PagedResult<ManagerDashboardDocumentDto>>> GetManagerRequestStatusDocumentsAsync(Guid managerId, DepartmentDocumentStatusFilterDto filter);
        Task<ApiResponse<PagedResult<IncomingAppraisalDocumentDto>>> GetIncomingAppraisalDocumentsAsync(Guid coordinatorId, PaginationDto pagination, string? searchTerm);
        Task<ApiResponse<PagedResult<IncomingAppraisalDocumentDto>>> GetUserWorkloadDocumentsAsync(Guid userId, PaginationDto pagination, string? searchTerm);
    }
}