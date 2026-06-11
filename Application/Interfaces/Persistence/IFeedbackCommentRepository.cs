using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IFeedbackCommentRepository : IRepository<FeedbackComment>
    {
        IQueryable<FeedbackComment> GetCommentsByIssueWithDetailsQuery(Guid issueId);
        Task<FeedbackComment?> GetCommentWithDetailsAsync(Guid id);
    }
}