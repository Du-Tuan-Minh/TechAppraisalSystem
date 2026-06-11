using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class AppraisalAssignmentDto
    {
        public Guid Id { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public AssignmentStatus Status { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentCode { get; set; } = string.Empty;
        public Guid RequestVersionId { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class AppraisalAssignmentDetailDto : AppraisalAssignmentDto
    {
        public Guid AssignedById { get; set; }
        public string AssignedByName { get; set; } = string.Empty;
        public Guid ResponsibleManagerId { get; set; }
        public string ResponsibleManagerName { get; set; } = string.Empty;
        public string? ManagerComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int ReviewerCount { get; set; }
        public int AttachmentCount { get; set; }
        public int IssueCount { get; set; }
        public List<AppraisalReviewerDto> Reviewers { get; set; } = new();
    }

    public class AssignStaffRequest
    {
        [Required]
        public Guid AssignmentId { get; set; }
        public List<Guid> StaffIds { get; set; } = new();
        public string? ManagerNote { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class CompleteAssignmentRequest
    {
        [Required]
        public Guid AssignmentId { get; set; }

        [Required]
        public string FinalComment { get; set; } = string.Empty;
        public List<Guid> ValidatedIssueIds { get; set; } = new();
        public List<Guid> RejectedIssueIds { get; set; } = new();
        public List<Guid> AttachmentIds { get; set; } = new();
    }

    public class ConsolidateAppraisalRequest
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid RequestVersionId { get; set; }
        public List<Guid> ValidatedIssueIds { get; set; } = new();
        public List<Guid> RejectedIssueIds { get; set; } = new();
        public string? FinalComment { get; set; }
        public bool IsPass { get; set; }
        public List<Guid> AttachmentIds { get; set; } = new();
    }

    public class CreateParallelAssignmentsRequest
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid RequestVersionId { get; set; }
        public List<DepartmentAssignmentInfo> DepartmentAssignments { get; set; } = new();
        public DateTime? GlobalDeadline { get; set; }
        public string? GlobalComment { get; set; }
    }

    public class DepartmentAssignmentInfo
    {
        public Guid TargetId { get; set; }
        public DateTime? Deadline { get; set; }
        public string? ManagerComment { get; set; }
    }

    public class SendParallelAssignmentsRequest
    {
        [Required]
        public Guid DocumentId { get; set; }
        [MaxLength(2000)]
        public string? Comment { get; set; }
    }

    public class CoordinatorAssignRequest
    {
        [Required]
        public Guid DocumentId { get; set; }
        public Guid VersionId { get; set; }
        public List<Guid>? ManagerIds { get; set; }
        public List<Guid>? StaffIds { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Comment { get; set; }
    }
}