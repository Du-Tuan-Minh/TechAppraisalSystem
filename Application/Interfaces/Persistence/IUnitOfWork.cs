using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRepository<Profile> Profiles { get; }
        IDepartmentRepository Departments { get; }
        IDepartmentInvitationRepository DepartmentInvitations { get; }
        ITechnicalDocumentRepository TechnicalDocument { get; }
        IRepository<RequestVersion> RequestVersions { get; }
        IAppraisalHistoryRepository AppraisalHistory { get; }
        IAttachmentRepository Attachments { get; }
        ITechnicalKnowledgeBaseRepository TechnicalKnowledgeBase { get; }
        IRepository<Notification> Notifications { get; }
        IUserNotificationRepository UserNotifications { get; }
        IFeedbackIssueRepository FeedbackIssues { get; }
        IAppraisalAssignmentRepository AppraisalAssignments { get; }
        IAppraisalReviewerRepository AppraisalReviewers { get; }
        IFeedbackCommentRepository FeedbackComments { get; }
        IAttachmentLinkRepository AttachmentLinks { get; }
        IApprovalWorkflowRepository ApprovalWorkflows { get; }
        IDashboardRepository Dashboard { get; }

        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        Task<T> ExecuteWithStrategyAsync<T>(Func<Task<T>> action);
        Task ExecuteWithStrategyAsync(Func<Task> action);
    }
}