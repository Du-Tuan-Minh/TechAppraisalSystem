using Application.Common;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
            : base(context, logger) { }

        public override async Task<User?> GetByIdAsync(object id, params Expression<Func<User, object>>[] includeProperties)
        {
            if (includeProperties.Length == 0)
            {
                return await _dbSet
                    .Include(u => u.Profile)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id.Equals(id));
            }
            return await base.GetByIdAsync(id, includeProperties);
        }

        public override async Task<IEnumerable<User>> FindAsync(
            Expression<Func<User, bool>> predicate,
            params Expression<Func<User, object>>[] includeProperties)
        {
            if (includeProperties.Length == 0)
            {
                return await _dbSet.AsNoTracking()
                    .Where(predicate)
                    .Include(u => u.Profile)
                    .Include(u => u.Department)
                    .ToListAsync();
            }
            return await base.FindAsync(predicate, includeProperties);
        }

        public async Task<IEnumerable<User>> GetAdminsAsync()
        {
            return await FindAsync(u => u.Role == UserRole.Admin);
        }

        public async Task<User?> GetUnitLeaderAsync(Guid departmentId)
        {
            return await _dbSet.AsNoTracking()
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u =>
                    u.DepartmentId == departmentId &&
                    u.Role == UserRole.Manager &&
                    u.IsActive);
        }

        public async Task<User?> GetDirectorByDepartmentAsync(Guid departmentId)
        {
            var dept = await _context.Set<Department>().AsNoTracking()
                .Select(d => new { d.Id, d.ParentId })
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (dept == null) return null;
            Guid targetCenterId = dept.ParentId ?? dept.Id;

            return await _dbSet.AsNoTracking()
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u =>
                    u.DepartmentId == targetCenterId &&
                    u.Role == UserRole.Director &&
                    u.IsActive);
        }

        public async Task<Guid?> GetFirstUserIdByRoleAsync(UserRole role, Guid? departmentId = null)
        {
            IQueryable<User> query = _dbSet.AsNoTracking()
                .Where(u => u.Role == role && u.IsActive);

            if (departmentId.HasValue && role == UserRole.Manager)
            {
                query = query.Where(u => u.DepartmentId == departmentId.Value);
            }

            return await query
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<User>> GetUsersFilteredAsync(
            Guid? departmentId,
            UserRole? role,
            bool? isActive,
            string? searchTerm,
            int page,
            int pageSize,
            bool includeSubDepartments = false,
            bool staffOnly = false)
        {
            var query = _dbSet.AsNoTracking()
                              .Include(u => u.Profile)
                              .Include(u => u.Department)
                              .AsQueryable();

            // 1. Lọc theo đơn vị (Xử lý đa cấp cho Director)
            if (departmentId.HasValue)
            {
                if (includeSubDepartments)
                {
                    // Nếu là Director: Lấy nhân viên tại Trung tâm đó HOẶC tại các phòng ban con
                    query = query.Where(u => u.DepartmentId == departmentId.Value ||
                                             (u.Department != null && u.Department.ParentId == departmentId.Value));
                }
                else
                {
                    // Nếu là Manager/Admin lọc cụ thể: Chỉ lấy đúng phòng ban đó
                    query = query.Where(u => u.DepartmentId == departmentId.Value);
                }
            }

            // 2. Lọc theo Vai trò
            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            // 3. Lọc theo trạng thái hoạt động
            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            if (staffOnly)
            {
                query = query.Where(u => u.Role == UserRole.Staff);
            }

            // 4. Tìm kiếm (Sử dụng EF.Functions.Like để tối ưu hóa nếu dùng SQL Server/Postgres)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = $"%{searchTerm.ToLower()}%";
                query = query.Where(u => EF.Functions.Like(u.EmployeeCode.ToLower(), term) ||
                                         (u.Profile != null && (EF.Functions.Like(u.Profile.FirstName.ToLower(), term) ||
                                                                EF.Functions.Like(u.Profile.LastName.ToLower(), term))));
            }

            // 5. Luôn nên có mặc định OrderBy để tránh lỗi phân trang không đồng nhất
            query = query.OrderByDescending(u => u.CreatedAt);

            return await query.ToPagedListAsync(page, pageSize);
        }

        public async Task<UserRole?> GetUserRoleByIdAsync(Guid userId)
        {
            return await _dbSet.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();
        }

        public IQueryable<User> GetSeniorCentersQueryable(string? searchTerm)
        {
            var query = _dbSet.AsNoTracking()
                              .Where(u => (u.Role == UserRole.DeputyInstituteDirector ||
                                           u.Role == UserRole.InstituteDirector) && u.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = $"%{searchTerm.Trim()}%";
                query = query.Where(u => EF.Functions.Like(u.EmployeeCode, term) ||
                                         EF.Functions.Like(u.Profile.FirstName, term) ||
                                         EF.Functions.Like(u.Profile.LastName, term));
            }

            return query.Include(u => u.Profile)
                        .OrderByDescending(u => u.CreatedAt);
        }

        public async Task<PagedResult<DocumentTypeStatisticDto>> GetTopRejectedDocumentTypesAsync(Guid managerId, PaginationDto pagination)
        {
            var manager = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == managerId);

            if (manager?.DepartmentId == null)
            {
                return new PagedResult<DocumentTypeStatisticDto>();
            }

            var query = _context.TechnicalDocuments
                .AsNoTracking()
                .Where(x =>
                    x.DepartmentId == manager.DepartmentId &&
                    (
                        x.Status == DocumentStatus.Rejected ||
                        x.Status == DocumentStatus.AdjustmentRequired
                    ));

            var dtoQuery = query
                .GroupBy(x => x.Type)
                .Select(g => new DocumentTypeStatisticDto
                {
                    Type = g.Key,
                    TotalDocuments = g.Count()
                })
                .OrderByDescending(x => x.TotalDocuments);

            return await dtoQuery.ToPagedListAsync(
                pagination.Page,
                pagination.PageSize);
        }

        public async Task<PagedResult<User>> GetDepartmentAppraisalWorkloadsAsync(PaginationDto pagination, string? searchTerm)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(x => x.Profile)
                .Include(x => x.Department)

                .Include(x => x.AssignedReviews)
                    .ThenInclude(r => r.Assignment)
                        .ThenInclude(a => a.Document)

                .Include(x => x.ManagedAssignments)
                    .ThenInclude(a => a.Document)

                .Where(x =>
                   x.IsActive &&
                   (
                      x.AssignedReviews.Any(r =>
                         (r.Status == ReviewerStatus.Pending ||
                          r.Status == ReviewerStatus.Reviewing)
                         &&
                         (r.Assignment.Document.Status == DocumentStatus.AppraisalPending ||
                          r.Assignment.Document.Status == DocumentStatus.Appraising)
                     )

                       ||

                      x.ManagedAssignments.Any(a =>
                         (a.Status == AssignmentStatus.Pending ||
                          a.Status == AssignmentStatus.InReview)
                         &&
                         (a.Document.Status == DocumentStatus.AppraisalPending ||
                          a.Document.Status == DocumentStatus.Appraising)
)
                   ));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();

                query = query.Where(x =>
                    EF.Functions.ILike(x.EmployeeCode, $"%{term}%")

                    ||

                    (x.Profile != null &&
                     EF.Functions.ILike(
                         (x.Profile.FirstName ?? "") + " " +
                         (x.Profile.LastName ?? ""),
                         $"%{term}%")));
            }

            return await query
                .OrderBy(x => x.EmployeeCode)
                .ToPagedListAsync(pagination.Page, pagination.PageSize);
        }
    }
}