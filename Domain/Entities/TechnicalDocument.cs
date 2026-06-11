using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("TechnicalDocuments")]
    public class TechnicalDocument : BaseEntity
    {
        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required, MaxLength(100)]
        public string DocumentCode { get; set; } = string.Empty;

        [Required]
        public IssueSeverity Priority { get; set; }

        [Required]
        public DocumentStatus Status { get; set; }

        [Required]
        public Guid RequesterId { get; set; }

        [Column(TypeName = "jsonb")]
        public string ExternalDepartmentIds { get; set; } = "[]";

        [Column(TypeName = "jsonb")]
        public string ApprovalProposerIds { get; set; } = "[]";

        public DocumentType Type { get; set; }
        public string? QrCode { get; set; }
        public Guid? CurrentHandlerId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; } = null!;

        [ForeignKey(nameof(RequesterId))]
        public virtual User Requester { get; set; } = null!;

        [ForeignKey(nameof(CurrentHandlerId))]
        public virtual User? CurrentHandler { get; set; }

        public virtual ICollection<RequestVersion> Versions { get; set; } = new List<RequestVersion>();
        public virtual ICollection<AppraisalHistory> AppraisalHistories { get; set; } = new List<AppraisalHistory>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public virtual ICollection<AppraisalAssignment> AppraisalAssignments { get; set; } = new List<AppraisalAssignment>();
        public virtual ICollection<FeedbackIssue> FeedbackIssues { get; set; } = new List<FeedbackIssue>();
    }
}