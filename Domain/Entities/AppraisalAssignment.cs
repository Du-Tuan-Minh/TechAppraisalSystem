using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("AppraisalAssignments")]
    public class AppraisalAssignment : BaseEntity
    {
        public Guid DocumentId { get; set; }
        public Guid AssignedById { get; set; }
        public Guid DepartmentId { get; set; }
        public DateTime? Deadline { get; set; }
        public Guid ResponsibleManagerId { get; set; }
        public Guid RequestVersionId { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
        public string? ManagerComment { get; set; }
        public DateTime? CompletedAt { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual TechnicalDocument Document { get; set; } = null!;

        [ForeignKey(nameof(ResponsibleManagerId))]
        public virtual User ResponsibleManager { get; set; } = null!;

        [ForeignKey(nameof(RequestVersionId))]
        public virtual RequestVersion Version { get; set; } = null!;

        [ForeignKey(nameof(AssignedById))]
        public virtual User AssignedBy { get; set; } = null!;

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; } = null!;

        public virtual ICollection<FeedbackIssue> FeedbackIssues { get; set; } = new List<FeedbackIssue>();
        public virtual ICollection<AppraisalHistory> AppraisalHistories { get; set; } = new List<AppraisalHistory>();
        public virtual ICollection<AppraisalReviewer> Reviewers { get; set; } = new List<AppraisalReviewer>();
    }
}