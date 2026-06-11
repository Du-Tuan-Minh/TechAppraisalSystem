using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<DepartmentInvitation> DepartmentInvitations { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<TechnicalDocument> TechnicalDocuments { get; set; } = null!;
        public DbSet<RequestVersion> RequestVersions { get; set; } = null!;
        public DbSet<AppraisalHistory> AppraisalHistories { get; set; } = null!;
        public DbSet<FeedbackIssue> FeedbackIssues { get; set; } = null!;
        public DbSet<TechnicalKnowledgeBase> TechnicalKnowledgeBases { get; set; } = null!;
        public DbSet<Attachment> Attachments { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<UserNotification> UserNotifications { get; set; } = null!;
        public DbSet<AppraisalAssignment> AppraisalAssignments { get; set; } = null!;
        public DbSet<AppraisalReviewer> AppraisalReviewers { get; set; } = null!;
        public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; } = null!;
        public DbSet<AttachmentLink> AttachmentLinks { get; set; } = null!;
        public DbSet<FeedbackComment> FeedbackComments { get; set; } = null!;

        public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => base.Database;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
                    var isDeletedProperty = Expression.Call(null, propertyMethodInfo!, parameter, Expression.Constant("IsDeleted"));
                    var compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                    var lambda = Expression.Lambda(compareExpression, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            // Department
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasIndex(d => d.CodeDepartment).IsUnique();
                entity.HasIndex(d => d.ParentId);

                entity.HasOne(d => d.Parent)
                    .WithMany(d => d.SubDepartments)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.Users)
                    .WithOne(u => u.Department)
                    .HasForeignKey(u => u.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.Documents)
                    .WithOne(doc => doc.Department)
                    .HasForeignKey(doc => doc.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.AppraisalAssignments)
                    .WithOne(a => a.Department)
                    .HasForeignKey(a => a.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.HandledIssues)
                    .WithOne(fi => fi.AssignedDepartment)
                    .HasForeignKey(fi => fi.AssignedDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.Invitations)
                    .WithOne(i => i.Department)
                    .HasForeignKey(i => i.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ApprovalWorkflow>(entity =>
            {
                entity.HasIndex(w => new { w.DocumentId, w.StepOrder });
                entity.HasIndex(w => w.IsCurrentStep);

                entity.Property(w => w.Status).HasConversion<string>();
                entity.Property(w => w.RequiredRole).HasConversion<string>();

                entity.HasOne(w => w.Document)
                    .WithMany()
                    .HasForeignKey(w => w.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Version)
                    .WithMany()
                    .HasForeignKey(w => w.RequestVersionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(w => w.Approver)
                    .WithMany()
                    .HasForeignKey(w => w.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(w => w.AppraisalAssignment)
                    .WithMany()
                    .HasForeignKey(w => w.AppraisalAssignmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DepartmentInvitation>(entity =>
            {
                entity.HasIndex(i => i.InvitationCode).IsUnique();
                entity.HasIndex(i => i.InviteeEmployeeCode);
            });

            // User & Profile 
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.EmployeeCode).IsUnique();
                entity.HasIndex(u => u.DepartmentId);
                entity.Property(u => u.Role).HasConversion<string>();

                entity.HasMany(u => u.ManagedAssignments)
                    .WithOne(a => a.ResponsibleManager)
                    .HasForeignKey(a => a.ResponsibleManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Comments)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithOne(u => u.Profile)
                    .HasForeignKey<Profile>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AppraisalAssignment
            modelBuilder.Entity<AppraisalAssignment>(entity =>
            {
                entity.HasIndex(a => a.DocumentId);
                entity.HasIndex(a => a.RequestVersionId);
                entity.HasIndex(a => a.ResponsibleManagerId);
                entity.HasIndex(a => new { a.DocumentId, a.RequestVersionId });
                entity.Property(a => a.Status).HasConversion<string>();

                entity.HasOne(a => a.Document)
                    .WithMany(d => d.AppraisalAssignments)
                    .HasForeignKey(a => a.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Version)
                    .WithMany(v => v.AppraisalAssignments)
                    .HasForeignKey(a => a.RequestVersionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Department)
                    .WithMany(dept => dept.AppraisalAssignments)
                    .HasForeignKey(a => a.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.AssignedBy)
                    .WithMany()
                    .HasForeignKey(a => a.AssignedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.ResponsibleManager)
                    .WithMany()
                    .HasForeignKey(a => a.ResponsibleManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AppraisalReviewer>(entity =>
            {
                entity.Property(r => r.Status).HasConversion<string>();

                entity.HasOne(r => r.Assignment).WithMany(a => a.Reviewers)
                    .HasForeignKey(r => r.AssignmentId).OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Staff).WithMany(u => u.AssignedReviews)
                    .HasForeignKey(r => r.StaffId).OnDelete(DeleteBehavior.Restrict);
            });

            // TechnicalDocument 
            modelBuilder.Entity<TechnicalDocument>(entity =>
            {
                entity.HasIndex(d => d.DocumentCode).IsUnique();
                entity.HasIndex(d => d.Status);
                entity.HasIndex(d => d.DepartmentId);

                entity.Property(d => d.Status).HasConversion<string>();
                entity.Property(d => d.Priority).HasConversion<string>();
                entity.Property(d => d.Type).HasConversion<string>();

                entity.Property(d => d.ExternalDepartmentIds)
                    .HasColumnType("jsonb")
                    .HasDefaultValue("[]");

                entity.Property(d => d.ApprovalProposerIds)
                    .HasColumnType("jsonb")
                    .HasDefaultValue("[]");

                entity.HasOne(d => d.Department)
                    .WithMany(dept => dept.Documents)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Requester)
                    .WithMany(u => u.RequestedDocuments)
                    .HasForeignKey(d => d.RequesterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.CurrentHandler)
                    .WithMany(u => u.HandlingDocuments)
                    .HasForeignKey(d => d.CurrentHandlerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RequestVersion
            modelBuilder.Entity<RequestVersion>(entity =>
            {
                entity.Property(rv => rv.TechnicalSpecsJson).HasColumnType("jsonb");
                entity.HasIndex(rv => rv.TechnicalSpecsJson).HasMethod("gin");

                entity.HasOne(rv => rv.OriginatingIssue)
                    .WithMany()
                    .HasForeignKey(rv => rv.SourceIssueId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(rv => rv.IssuesReported)
                    .WithOne(fi => fi.Version)
                    .HasForeignKey(fi => fi.RequestVersionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(rv => rv.IssuesResolved)
                    .WithOne(fi => fi.ResolvedInVersion)
                    .HasForeignKey(fi => fi.ResolvedInVersionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(rv => new { rv.RequestId, rv.VersionNumber }).IsUnique();
            });

            // Feedback & Knowledge Base 
            modelBuilder.Entity<AppraisalHistory>(entity =>
            {
                entity.Property(ah => ah.OldStatus).HasConversion<string>();
                entity.Property(ah => ah.NewStatus).HasConversion<string>();

                entity.HasOne(ah => ah.Handler)
                    .WithMany(u => u.AppraisalHistories)
                    .HasForeignKey(ah => ah.HandlerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ah => ah.RequestVersion)
                    .WithMany(rv => rv.AppraisalHistories)
                    .HasForeignKey(ah => ah.RequestVersionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ah => ah.Document)
                    .WithMany(d => d.AppraisalHistories)
                    .HasForeignKey(ah => ah.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ah => ah.AppraisalAssignment)
                    .WithMany(a => a.AppraisalHistories)
                    .HasForeignKey(ah => ah.AppraisalAssignmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<FeedbackIssue>(entity =>
            {
                entity.HasIndex(fi => fi.DocumentId);
                entity.HasIndex(fi => fi.RequestVersionId);
                entity.HasIndex(fi => fi.Status);
                entity.HasIndex(fi => fi.AssignedDepartmentId);
                entity.HasIndex(fi => new { fi.DocumentId, fi.Status });

                entity.Property(fi => fi.Severity).HasConversion<string>();
                entity.Property(fi => fi.Status).HasConversion<string>();
                entity.Property(fi => fi.IssueCategory).HasConversion<string>();

                entity.HasOne(fi => fi.Reporter)
                    .WithMany(u => u.ReportedIssues)
                     .HasForeignKey(fi => fi.ReporterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(fi => fi.AssignedDepartment)
                    .WithMany(d => d.HandledIssues)
                    .HasForeignKey(fi => fi.AssignedDepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(fi => fi.AppraisalHistory)
                    .WithMany(ah => ah.FeedbackIssues)
                    .HasForeignKey(fi => fi.AppraisalHistoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(fi => fi.Document)
                    .WithMany(d => d.FeedbackIssues)
                    .HasForeignKey(fi => fi.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);

                //entity.HasMany(fi => fi.AttachmentLinks)
                //    .WithOne() 
                //    .HasForeignKey(al => al.EntityId)
                //    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FeedbackComment>(entity =>
            {
                entity.HasIndex(c => c.FeedbackIssueId);
                entity.HasIndex(c => c.ParentCommentId);

                entity.HasOne(c => c.FeedbackIssue)
                    .WithMany(fi => fi.Comments)
                    .HasForeignKey(c => c.FeedbackIssueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Attachment  && AttachmentLink
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.Property(a => a.ContentCategory).HasConversion<string>();
                entity.Property(a => a.FileData).IsRequired();

                entity.HasOne(a => a.TechnicalDocument)
                    .WithMany(d => d.Attachments)
                    .HasForeignKey(a => a.TechnicalDocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Uploader)
                    .WithMany(u => u.UploadedAttachments)
                    .HasForeignKey(a => a.UploadedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AttachmentLink>(entity =>
            {
                entity.HasIndex(al => new { al.EntityId, al.EntityType });
                entity.HasIndex(al => new { al.EntityId, al.EntityType, al.AttachmentId }).IsUnique();
                entity.Property(al => al.EntityType).HasConversion<string>();

                entity.HasOne(al => al.Attachment)
                    .WithMany(a => a.Links)
                    .HasForeignKey(al => al.AttachmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification
            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.HasKey(un => un.Id);
                entity.HasIndex(un => new { un.UserId, un.NotificationId }).IsUnique();

                entity.HasOne(un => un.User).WithMany(u => u.UserNotifications).HasForeignKey(un => un.UserId).OnDelete(DeleteBehavior.Cascade); ;
                entity.HasOne(un => un.Notification).WithMany(n => n.UserNotifications).HasForeignKey(un => un.NotificationId).OnDelete(DeleteBehavior.Cascade); ;
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(n => n.Type).HasConversion<string>();
                entity.Property(n => n.Metadata).HasColumnType("jsonb");
            });

            modelBuilder.Entity<TechnicalKnowledgeBase>(entity =>
            {
                entity.Property(tk => tk.LinkedSpecPattern).HasColumnType("jsonb");
                entity.HasIndex(tk => tk.LinkedSpecPattern).HasMethod("gin");
                entity.Property(tk => tk.Severity).HasConversion<string>();
                entity.HasIndex(tk => tk.Title);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    // 1. Tự động cập nhật thời gian khi thêm mới
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.IsDeleted = false;
                        break;

                    // 2. Cập nhật thời gian khi sửa đổi
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;

                    // 3. Chuyển xóa thật thành Soft Delete
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; // Chặn hành động xóa
                        entry.Entity.IsDeleted = true;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}