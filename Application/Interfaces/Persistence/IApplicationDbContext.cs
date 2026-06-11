using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<Department> Departments { get; }
        DbSet<DepartmentInvitation> DepartmentInvitations { get; }
        DbSet<User> Users { get; }
        DbSet<Profile> Profiles { get; }
        DbSet<TechnicalDocument> TechnicalDocuments { get; }
        DbSet<RequestVersion> RequestVersions { get; }
        DbSet<AppraisalHistory> AppraisalHistories { get; }
        DbSet<Attachment> Attachments { get; }
        DbSet<TechnicalKnowledgeBase> TechnicalKnowledgeBases { get; }
        DbSet<FeedbackIssue> FeedbackIssues { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<UserNotification> UserNotifications { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}