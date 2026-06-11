using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ApprovalWorkflowResponseDto
    {
        public Guid Id { get; set; }
        public int StepOrder { get; set; }
        public UserRole RequiredRole { get; set; }
        public AssignmentStatus Status { get; set; }
        public bool IsCurrentStep { get; set; }
        public DateTime? SignedAt { get; set; }
        public Guid? ApproverId { get; set; }
        public string? ApproverName { get; set; }
    }

    public class ApprovalWorkflowDetailDto : ApprovalWorkflowResponseDto
    {
        public DateTime? Deadline { get; set; }
        public string? Note { get; set; }
        public Guid DocumentId { get; set; }
        public Guid RequestVersionId { get; set; }
        public Guid? AppraisalAssignmentId { get; set; }
    }

    public class ApprovalActionRequestDto
    {
        [Required]
        public Guid ApprovalWorkflowId { get; set; }

        public bool IsApproved { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class CreateWorkflowRequest
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid RequestVersionId { get; set; }

        public Guid? AppraisalAssignmentId { get; set; }

        [MinLength(1)]
        public List<WorkflowStepConfigDto> Steps { get; set; } = new();
    }

    public class WorkflowStepConfigDto
    {
        [Range(1, int.MaxValue)]
        public int StepOrder { get; set; }

        [EnumDataType(typeof(UserRole))]
        public UserRole RequiredRole { get; set; }
        public Guid? ApproverId { get; set; }
        public DateTime? Deadline { get; set; }
    }
}