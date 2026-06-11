using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    //public class FeedbackActionRequestDto
    //{
    //    [Required]
    //    public Guid IssueId { get; set; }

    //    [Required]
    //    public FeedbackAction Action { get; set; }

    //    [Required(ErrorMessage = "Phải có nội dung phản hồi")]
    //    public string Content { get; set; } = string.Empty;

    //    public List<Guid> AttachmentIds { get; set; } = new();
    //    public Dictionary<string, object> NewTechnicalSpecs { get; set; }
    //}

    public class FeedbackIssueUpdateDto
    {
        [Required]
        public IssueStatus Status { get; set; }

        public Guid? AssignedDepartmentId { get; set; }
        public Guid? ResolvedInVersionId { get; set; }

        [MaxLength(1000)]
        public string? ResolutionNote { get; set; }

        public IssueCategory? IssueCategory { get; set; }
        public IssueSeverity? Severity { get; set; }
    }

    public class FeedbackIssueCreateDto
    {
        [Required]
        public Guid DocumentId { get; set; }

        public string IndicatorPath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả lỗi không được để trống")]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public IssueCategory IssueCategory { get; set; }

        [Required]
        public IssueSeverity Severity { get; set; } = IssueSeverity.Minor;

        public Guid? TechnicalKnowledgeBaseId { get; set; }

        public Guid? AppraisalHistoryId { get; set; }
    }

    public class FeedbackIssueResponseDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid RequestVersionId { get; set; }
        public int VersionNumber { get; set; }
        public IssueCategory IssueCategory { get; set; }
        public IssueSeverity Severity { get; set; }
        public IssueStatus Status { get; set; }
        public Guid? AssignedDepartmentId { get; set; }
        public string IndicatorPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AssignedDepartmentName { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
    }

    public class FeedbackIssueDetailDto : FeedbackIssueResponseDto
    {
        public Guid ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public Guid? TechnicalKnowledgeBaseId { get; set; }
        public string? KnowledgeBaseTitle { get; set; }
        public Guid? ResolvedInVersionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AttachmentCount { get; set; }
        public List<AttachmentResponseDto>? Attachments { get; set; }
    }

    public class FeedbackResponseDto
    {
        [Required]
        public FeedbackAction Action { get; set; }

        [Required(ErrorMessage = "Nội dung nhận xét không được để trống")]
        public string Comment { get; set; } = string.Empty;
        public List<FeedbackIssueCreateDto>? NewIssues { get; set; } = new();
        public List<Guid>? AttachmentIds { get; set; } = new();
    }
}