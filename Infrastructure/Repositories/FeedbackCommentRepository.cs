using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class FeedbackCommentRepository : BaseRepository<FeedbackComment>, IFeedbackCommentRepository
    {
        public FeedbackCommentRepository(ApplicationDbContext context, ILogger<BaseRepository<FeedbackComment>> logger)
            : base(context, logger) { }

        public IQueryable<FeedbackComment> GetCommentsByIssueWithDetailsQuery(Guid issueId)
        {
            return _dbSet
                .AsNoTracking()
                .Where(c => c.FeedbackIssueId == issueId && c.ParentCommentId == null)
                .Include(c => c.User).ThenInclude(u => u.Profile)
                .Include(c => c.AttachmentLinks).ThenInclude(al => al.Attachment)
                .Include(c => c.Replies.OrderBy(r => r.CreatedAt))
                    .ThenInclude(r => r.User).ThenInclude(up => up.Profile)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.AttachmentLinks).ThenInclude(ral => ral.Attachment)
                .OrderByDescending(c => c.CreatedAt);
        }

        public async Task<FeedbackComment?> GetCommentWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.User).ThenInclude(u => u.Profile)
                .Include(c => c.AttachmentLinks).ThenInclude(al => al.Attachment)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}