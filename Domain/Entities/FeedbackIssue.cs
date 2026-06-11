using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("FeedbackIssues")]
    public class FeedbackIssue : BaseEntity
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid RequestVersionId { get; set; }

        [Required]
        public Guid ReporterId { get; set; }

        [Required, MaxLength(1000)]
        public string IndicatorPath { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public IssueCategory IssueCategory { get; set; }

        [Required]
        public IssueSeverity Severity { get; set; }

        public IssueStatus Status { get; set; } = IssueStatus.New;
        public Guid? AppraisalHistoryId { get; set; }
        public Guid? TechnicalKnowledgeBaseId { get; set; }
        public Guid? ResolvedInVersionId { get; set; }
        public Guid? AssignedDepartmentId { get; set; }

        [ForeignKey(nameof(RequestVersionId))]
        public virtual RequestVersion Version { get; set; } = null!;

        [ForeignKey(nameof(AppraisalHistoryId))]
        public virtual AppraisalHistory? AppraisalHistory { get; set; }

        [ForeignKey(nameof(TechnicalKnowledgeBaseId))]
        public virtual TechnicalKnowledgeBase? TechnicalKnowledgeBase { get; set; }

        [ForeignKey(nameof(ResolvedInVersionId))]
        public virtual RequestVersion? ResolvedInVersion { get; set; }

        [ForeignKey(nameof(AssignedDepartmentId))]
        public virtual Department? AssignedDepartment { get; set; }

        [ForeignKey(nameof(ReporterId))]
        public virtual User Reporter { get; set; } = null!;

        [ForeignKey(nameof(DocumentId))]
        public virtual TechnicalDocument Document { get; set; } = null!;

        public virtual ICollection<AttachmentLink> AttachmentLinks { get; set; } = new List<AttachmentLink>();
        public virtual ICollection<FeedbackComment> Comments { get; set; } = new List<FeedbackComment>();
    }
}