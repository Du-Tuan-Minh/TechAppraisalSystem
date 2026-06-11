using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("AppraisalHistories")]
    public class AppraisalHistory : BaseEntity
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid HandlerId { get; set; }

        [Required]
        public Guid RequestVersionId { get; set; }

        [Required]
        public DocumentStatus OldStatus { get; set; }

        [Required]
        public DocumentStatus NewStatus { get; set; }

        public Guid? AppraisalAssignmentId { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual TechnicalDocument Document { get; set; } = null!;

        [ForeignKey(nameof(HandlerId))]
        public virtual User Handler { get; set; } = null!;

        [ForeignKey(nameof(RequestVersionId))]
        public virtual RequestVersion? RequestVersion { get; set; }

        [ForeignKey(nameof(AppraisalAssignmentId))]
        public virtual AppraisalAssignment? AppraisalAssignment { get; set; }
        public virtual ICollection<FeedbackIssue> FeedbackIssues { get; set; } = new List<FeedbackIssue>();
    }
}