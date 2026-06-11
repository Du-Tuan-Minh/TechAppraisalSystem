using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Persistence
{
    public interface IAppraisalAssignmentRepository : IRepository<AppraisalAssignment>
    {
        Task<AppraisalAssignment?> GetWithDetailsAsync(Guid id);
        Task<PagedResult<AppraisalAssignment>> GetDirectorAssignmentsAsync(Guid directorId, AssignmentStatus? status, int page, int pageSize);
        Task<PagedResult<AppraisalAssignment>> GetManagerAssignmentsAsync(Guid currentUserId, Guid currentDeptId, Guid? versionId, UserRole role, int page, int pageSize);
    }
}
