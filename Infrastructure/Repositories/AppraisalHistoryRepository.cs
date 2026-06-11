using Application.Common;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class AppraisalHistoryRepository : BaseRepository<AppraisalHistory>, IAppraisalHistoryRepository
    {
        public AppraisalHistoryRepository(ApplicationDbContext context, ILogger<AppraisalHistoryRepository> logger)
            : base(context, logger) { }

        public async Task<PagedResult<AppraisalHistory>> GetHistoryByDocumentIdPagedAsync(Guid documentId, PaginationDto pagination)
        {
            var query = _dbSet.AsNoTracking()
                .Where(h => h.DocumentId == documentId)
                .Include(h => h.Handler).ThenInclude(u => u.Profile)
                .Include(h => h.RequestVersion)
                .OrderByDescending(h => h.CreatedAt);

            return await query.ToPagedListAsync(pagination.Page, pagination.PageSize);
        }

        public async Task<AppraisalHistory?> GetHistoryByVersionId(Guid versionId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(h => h.RequestVersionId == versionId)
                .Include(h => h.Handler)
                    .ThenInclude(u => u.Profile)
                .Include(h => h.Document)
                .Include(h => h.FeedbackIssues)
                .OrderByDescending(h => h.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}