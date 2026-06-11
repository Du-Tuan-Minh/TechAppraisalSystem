using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class FeedbackIssueRepository : BaseRepository<FeedbackIssue>, IFeedbackIssueRepository
    {
        public FeedbackIssueRepository(ApplicationDbContext context, ILogger<FeedbackIssueRepository> logger)
            : base(context, logger) { }

        public IQueryable<FeedbackIssue> GetFeedbackByDocumentIdAsync(Guid documentId)
        {
            return _dbSet.AsNoTracking()
                .Where(i => i.DocumentId == documentId)
                .Include(i => i.Reporter).ThenInclude(u => u.Profile)
                .Include(i => i.Version)
                .OrderByDescending(i => i.CreatedAt);
        }

        public async Task<FeedbackIssue?> GetDetailWithReporterAsync(Guid id)
        {
            return await _dbSet.AsNoTracking()
                .Include(i => i.Reporter).ThenInclude(u => u.Profile)
                .Include(i => i.Document)
                .Include(i => i.Version)
                .Include(i => i.AttachmentLinks)
                    .ThenInclude(al => al.Attachment)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public IQueryable<FeedbackIssue> GetByVersionIdAsync(Guid versionId)
        {
            return _dbSet.AsNoTracking()
                .Where(i => i.RequestVersionId == versionId)
                .Include(i => i.Reporter).ThenInclude(u => u.Profile)
                .Include(i => i.Version)
                .OrderByDescending(i => i.CreatedAt);
        }
    }
}
