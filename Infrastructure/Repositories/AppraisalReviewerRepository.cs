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
    public class AppraisalReviewerRepository : BaseRepository<AppraisalReviewer>, IAppraisalReviewerRepository
    {
        public AppraisalReviewerRepository(ApplicationDbContext context, ILogger<AppraisalReviewerRepository> logger)
            : base(context, logger) { }

        public async Task<bool> IsStaffAssignedAsync(Guid assignmentId, Guid staffId)
        {
            return await _dbSet.AnyAsync(r => r.AssignmentId == assignmentId && r.StaffId == staffId);
        }

        public async Task<AppraisalReviewer?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(r => r.Staff)
                    .ThenInclude(u => u.Profile)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Department)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Document)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<PagedResult<AppraisalReviewer>> GetByReviewerIdAsync(Guid? assignmentId, Guid userId, UserRole role, PaginationDto pagination)
        {
            IQueryable<AppraisalReviewer> query = _dbSet.AsNoTracking()
                .Include(r => r.Staff)
                    .ThenInclude(u => u.Profile);

            if (role == UserRole.Manager || role == UserRole.Director)
            {
                if (assignmentId.HasValue)
                    query = query.Where(r => r.AssignmentId == assignmentId.Value);
            }
            else if (role == UserRole.Staff)
            {
                query = query.Where(r => r.StaffId == userId);
                if (assignmentId.HasValue)
                    query = query.Where(r => r.AssignmentId == assignmentId.Value);
            }

            query = query.OrderByDescending(r => r.CreatedAt);

            return await query.ToPagedListAsync(pagination.Page, pagination.PageSize);
        }
    }
}