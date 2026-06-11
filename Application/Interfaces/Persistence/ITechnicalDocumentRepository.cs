using Application.Common;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Persistence
{
    public interface ITechnicalDocumentRepository : IRepository<TechnicalDocument>
    {
        Task<TechnicalDocument?> GetDetailDocumentAsync(Guid id);
        Task<bool> IsOwnerAsync(Guid documentId, Guid userId);
        Task<PagedResult<TechnicalDocument>> GetDocumentsFilteredAsync(DocumentFilterDto filter, Guid userId, UserRole role, Guid? userDepartmentId);
        Task<PagedResult<TechnicalDocument>> GetMyTasksAsync(Guid userId, PaginationDto pagination);
        Task<TechnicalDocument?> GetVersionWithReviewersAsync(Guid versionId);
        Task<PagedResult<TechnicalDocument>> GetMyCurrentDocumentsAsync(Guid userId, UserCurrentDocumentFilterDto filter);
        //Task<PagedResult<AppraisalReviewer>> GetPendingAppraisalResponsesAsync(Guid managerId, PendingAppraisalFilterDto filter);
        Task<PagedResult<AppraisalReviewer>> GetOverdueDocumentsAsync(Guid managerId, OverdueFilterDto filter);
        Task<PagedResult<TechnicalDocument>> GetManagerStatusDocumentsAsync(Guid managerId, ManagerDashboardDocumentFilterDto filter);
        Task<PagedResult<AppraisalReviewer>> GetManagerRequestOverdueDocumentsAsync(Guid directorId, OverdueFilterDto filter);
        Task<PagedResult<TechnicalDocument>> GetManagerRequestStatusDocumentsAsync(Guid managerId, DepartmentDocumentStatusFilterDto filter);
        Task<PagedResult<TechnicalDocument>> GetIncomingAppraisalDocumentsAsync(Guid departmentId, PaginationDto pagination, string? searchTerm);
        Task<PagedResult<TechnicalDocument>> GetUserWorkloadDocumentsAsync(Guid userId, PaginationDto pagination, string? searchTerm);
    }
}