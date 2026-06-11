using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context, ILogger<DepartmentRepository> logger)
            : base(context, logger) { }

        public IQueryable<Department> GetDepartmentsQueryable(string? searchTerm)
        {
            var query = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = $"%{searchTerm.Trim()}%";
                query = query.Where(d => EF.Functions.Like(d.NameDepartment, term) ||
                                         EF.Functions.Like(d.CodeDepartment, term));
            }

            return query.Include(d => d.Users.Where(u => u.Role == UserRole.Manager && u.IsActive))
                            .ThenInclude(u => u.Profile)
                        .OrderByDescending(d => d.CreatedAt);
        }

        public async Task<Guid?> GetCenterIdByCodeAsync(string parentCode)
        {
            if (string.IsNullOrWhiteSpace(parentCode)) return null;

            return await _dbSet.AsNoTracking()
                .Where(d => d.ParentId == null && d.CodeDepartment == parentCode)
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync();
        }
    }
}