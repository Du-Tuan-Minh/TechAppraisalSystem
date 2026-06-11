using Application.Common;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class AppraisalAssignmentRepository : BaseRepository<AppraisalAssignment>, IAppraisalAssignmentRepository
    {
        public AppraisalAssignmentRepository(ApplicationDbContext context, ILogger<BaseRepository<AppraisalAssignment>> logger)
            : base(context, logger) { }

        public async Task<AppraisalAssignment?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet.AsNoTracking()
                .Include(a => a.Document)
                .Include(a => a.Version)
                .Include(a => a.Department)
                .Include(a => a.AssignedBy).ThenInclude(u => u.Profile)
                .Include(a => a.ResponsibleManager).ThenInclude(u => u.Profile)
                .Include(a => a.Reviewers)
                    .ThenInclude(r => r.Staff)
                        .ThenInclude(u => u.Profile)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<PagedResult<AppraisalAssignment>> GetDirectorAssignmentsAsync(Guid directorId, AssignmentStatus? status, int page, int pageSize)
        {
            var query = _dbSet.AsNoTracking()
                .Include(a => a.Document)
                .Include(a => a.Version)
                .Include(a => a.Department)
                .Include(a => a.ResponsibleManager)
                .Where(a => a.ResponsibleManagerId == directorId)
                .AsQueryable();

            if (status.HasValue) query = query.Where(a => a.Status == status.Value);
            query = query.OrderByDescending(a => a.CreatedAt);
            return await query.ToPagedListAsync(page, pageSize);
        }

        public async Task<PagedResult<AppraisalAssignment>> GetManagerAssignmentsAsync(Guid currentUserId, Guid currentDeptId, Guid? versionId, UserRole role, int page, int pageSize)
        {
            var query = _dbSet.AsNoTracking()
                .Include(a => a.Document)
                .Include(a => a.Version)
                .Include(a => a.Department)
                .Include(a => a.ResponsibleManager)
                .AsQueryable();

            switch (role)
            {
                case UserRole.Manager:
                    query = query.Where(a => a.DepartmentId == currentDeptId);
                    break;

                case UserRole.Director:
                    query = query.Where(a => a.Department.ParentId == currentDeptId);
                    break;

                case UserRole.InstituteDirector:
                    query = query.Where(a =>
                        a.DepartmentId == currentDeptId &&
                        a.ResponsibleManager.Role == UserRole.DeputyInstituteDirector);
                    break;

                case UserRole.DeputyInstituteDirector:
                    query = query.Where(a =>
                         a.ResponsibleManagerId == currentUserId);
                    break;

                default:
                    query = query.Where(a => false);
                    break;
            }

            if (versionId.HasValue)
            {
                query = query.Where(a => a.RequestVersionId == versionId.Value);
            }

            query = query.OrderByDescending(a => a.CreatedAt);

            return await query.ToPagedListAsync(page, pageSize);
        }
    }
}