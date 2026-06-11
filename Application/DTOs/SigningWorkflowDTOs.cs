using Domain.Enums;

namespace Application.DTOs
{
    public class ApprovalStepResponseDto
    {
        public Guid Id { get; set; }
        public int StepOrder { get; set; }
        public UserRole RequiredRole { get; set; }
        public string? ApproverName { get; set; }
        public AssignmentStatus Status { get; set; }
        public bool IsCurrentStep { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? Note { get; set; }
        public Guid? AppraisalAssignmentId { get; set; }
    }

    public class SigningDetailResponseDto
    {
        public Guid DocumentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DocumentStatus CurrentStatus { get; set; }
        public List<ApprovalStepResponseDto> WorkflowSteps { get; set; } = new();
    }

    public class SigningWorkflowResponseDto
    {
        public Guid DocumentId { get; set; }
        public Guid VersionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DocumentStatus CurrentStatus { get; set; }
        public string? CreatorName { get; set; } 
        public DateTime CreatedAt { get; set; }
        public Guid? CurrentHandlerId { get; set; }
        public string? CurrentHandlerName { get; set; }
        public List<ApprovalStepResponseDto> WorkflowSteps { get; set; } = new();
        public List<ReviewerInfoDTO> Reviewers { get; set; } = new();
        public List<DepartmentAppraisalProgressDto> DepartmentAppraisals { get; set; } = new();
        public List<WorkflowVersionEventDto> VersionTimeline { get; set; } = new();
    }

    public class ReviewerInfoDTO
    {
        public Guid Id { get; set; }
        public Guid AssignmentId { get; set; }
        public Guid StaffId { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public ReviewerStatus Status { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Comment { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class DepartmentAppraisalProgressDto
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? ResponsibleManagerName { get; set; }
        public AssignmentStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ManagerComment { get; set; }
        public List<ReviewerInfoDTO> Reviewers { get; set; } = new();
    }

    public class WorkflowVersionEventDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ActorName { get; set; }
        public UserRole ActorRole { get; set; }
        public DocumentStatus OldStatus { get; set; }
        public DocumentStatus NewStatus { get; set; }
        public string? Comment { get; set; }
    }
}
