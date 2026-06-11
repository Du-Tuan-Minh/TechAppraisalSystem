using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ApprovalWorkflows")]
    public class ApprovalWorkflow : BaseEntity
    {
        public Guid DocumentId { get; set; }
        public Guid RequestVersionId { get; set; }
        public Guid? AppraisalAssignmentId { get; set; }
        public int StepOrder { get; set; }
        public UserRole RequiredRole { get; set; }
        public Guid? ApproverId { get; set; }
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;
        public string? Note { get; set; }
        public DateTime? SignedAt { get; set; }
        public bool IsCurrentStep { get; set; } = false;
        public DateTime? Deadline { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual TechnicalDocument Document { get; set; } = null!;

        [ForeignKey(nameof(RequestVersionId))]
        public virtual RequestVersion Version { get; set; } = null!;

        [ForeignKey(nameof(ApproverId))]
        public virtual User? Approver { get; set; }

        [ForeignKey(nameof(AppraisalAssignmentId))]
        public virtual AppraisalAssignment? AppraisalAssignment { get; set; }
    }
}