using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("AppraisalReviewers")]
    public class AppraisalReviewer : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public Guid StaffId { get; set; }
        public ReviewerStatus Status { get; set; } = ReviewerStatus.Reviewing;
        public string? TaskDescription { get; set; }
        public string? Comment { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public virtual AppraisalAssignment Assignment { get; set; } = null!;

        [ForeignKey(nameof(StaffId))]
        public virtual User Staff { get; set; } = null!;
    }
}
