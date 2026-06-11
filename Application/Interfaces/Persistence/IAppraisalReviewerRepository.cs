using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Persistence
{
    public interface IAppraisalReviewerRepository : IRepository<AppraisalReviewer>
    {
        Task<bool> IsStaffAssignedAsync(Guid assignmentId, Guid staffId);
        Task<AppraisalReviewer?> GetWithDetailsAsync(Guid id);
        Task<PagedResult<AppraisalReviewer>> GetByReviewerIdAsync(Guid? assignmentId, Guid userId, UserRole role, PaginationDto pagination);
    }
}
