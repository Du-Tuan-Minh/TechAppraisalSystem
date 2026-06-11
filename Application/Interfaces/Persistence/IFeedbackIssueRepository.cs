using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IFeedbackIssueRepository : IRepository<FeedbackIssue>
    {
        IQueryable<FeedbackIssue> GetFeedbackByDocumentIdAsync(Guid documentId);
        Task<FeedbackIssue?> GetDetailWithReporterAsync(Guid id);
        IQueryable<FeedbackIssue> GetByVersionIdAsync(Guid versionId);
    }
}