using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Users")]
    public class User : BaseEntity
    {
        [Required, MaxLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string HashPassword { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        public Guid? DepartmentId { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public virtual Profile? Profile { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        public virtual ICollection<AppraisalHistory> AppraisalHistories { get; set; } = new List<AppraisalHistory>();
        public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
        public virtual ICollection<FeedbackIssue> ReportedIssues { get; set; } = new List<FeedbackIssue>();
        public virtual ICollection<Attachment> UploadedAttachments { get; set; } = new List<Attachment>();
        public virtual ICollection<TechnicalKnowledgeBase> VerifiedKnowledgeBases { get; set; } = new List<TechnicalKnowledgeBase>();

        [InverseProperty(nameof(AppraisalReviewer.Staff))]
        public virtual ICollection<AppraisalReviewer> AssignedReviews { get; set; } = new List<AppraisalReviewer>();

        [InverseProperty(nameof(AppraisalAssignment.ResponsibleManager))]
        public virtual ICollection<AppraisalAssignment> ManagedAssignments { get; set; } = new List<AppraisalAssignment>();
        [InverseProperty(nameof(TechnicalDocument.Requester))]
        public virtual ICollection<TechnicalDocument> RequestedDocuments { get; set; } = new List<TechnicalDocument>();

        [InverseProperty(nameof(TechnicalDocument.CurrentHandler))]
        public virtual ICollection<TechnicalDocument> HandlingDocuments { get; set; } = new List<TechnicalDocument>();

        public virtual ICollection<FeedbackComment> Comments { get; set; } = new List<FeedbackComment>();
    }
}