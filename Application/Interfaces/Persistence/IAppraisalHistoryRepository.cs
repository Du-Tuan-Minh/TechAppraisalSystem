using Application.Common;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IAppraisalHistoryRepository : IRepository<AppraisalHistory>
    {
        Task<PagedResult<AppraisalHistory>> GetHistoryByDocumentIdPagedAsync(Guid documentId, PaginationDto pagination);
        Task<AppraisalHistory?> GetHistoryByVersionId(Guid versionId);
    }
}
