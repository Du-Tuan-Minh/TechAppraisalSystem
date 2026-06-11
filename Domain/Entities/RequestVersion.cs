using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("RequestVersions")]
    public class RequestVersion : BaseEntity
    {
        [Required]
        public Guid RequestId { get; set; }

        [Required]
        public int VersionNumber { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public string TechnicalSpecsJson { get; set; } = "{}";

        [MaxLength(1000)]
        public string? ChangeReason { get; set; }

        public Guid? SourceIssueId { get; set; }
        public bool IsCurrent { get; set; } = false;

        [ForeignKey(nameof(RequestId))]
        public virtual TechnicalDocument Request { get; set; } = null!;

        [ForeignKey(nameof(SourceIssueId))]
        public virtual FeedbackIssue? OriginatingIssue { get; set; }

        public virtual ICollection<AppraisalHistory> AppraisalHistories { get; set; } = new List<AppraisalHistory>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public virtual ICollection<AppraisalAssignment> AppraisalAssignments { get; set; } = new List<AppraisalAssignment>();
        public virtual ICollection<FeedbackIssue> IssuesReported { get; set; } = new List<FeedbackIssue>();

        [InverseProperty(nameof(FeedbackIssue.ResolvedInVersion))]
        public virtual ICollection<FeedbackIssue> IssuesResolved { get; set; } = new List<FeedbackIssue>();
    }
}