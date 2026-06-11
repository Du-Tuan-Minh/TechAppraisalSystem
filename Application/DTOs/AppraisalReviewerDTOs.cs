using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class AppraisalReviewerDto
    {
        public Guid Id { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public ReviewerStatus Status { get; set; }
    }

    public class AppraisalReviewerDetailDto: AppraisalReviewerDto
    {
        public Guid AssignmentId { get; set; }
        public Guid StaffId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? TaskDescription { get; set; }
        public string? Comment { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int IssueCount { get; set; }
        public int AttachmentCount { get; set; }
    }

    public class UpdateReviewerProgressRequest
    {
        [Required]
        public Guid ReviewerId { get; set; }

        [Required]
        public ReviewerStatus Status { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }
        public List<FeedbackIssueCreateDto> NewIssues { get; set; } = new();
        public List<Guid> AttachmentIds { get; set; } = new();
    }
}