using Application.Common;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Persistence
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IEnumerable<User>> GetAdminsAsync();
        Task<UserRole?> GetUserRoleByIdAsync(Guid userId);
        Task<User?> GetDirectorByDepartmentAsync(Guid departmentId);
        Task<User?> GetUnitLeaderAsync(Guid departmentId);
        Task<Guid?> GetFirstUserIdByRoleAsync(UserRole role, Guid? departmentId = null);
        Task<PagedResult<User>> GetUsersFilteredAsync(Guid? departmentId, UserRole? role, bool? isActive, string? searchTerm, int page, int pageSize, bool includeSubDepartments = false, bool staffOnly = false);
        IQueryable<User> GetSeniorCentersQueryable(string? searchTerm);
        Task<PagedResult<DocumentTypeStatisticDto>> GetTopRejectedDocumentTypesAsync(Guid managerId, PaginationDto pagination);
        Task<PagedResult<User>> GetDepartmentAppraisalWorkloadsAsync(PaginationDto pagination, string? searchTerm);
    }
}
