using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<Guid?> GetCenterIdByCodeAsync(string parentCode);
        IQueryable<Department> GetDepartmentsQueryable(string? searchTerm);
    }
}
