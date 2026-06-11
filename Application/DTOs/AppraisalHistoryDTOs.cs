using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class AppraisalHistoryResponseDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public Guid RequestVersionId { get; set; }
        public int VersionNumber { get; set; }
        public Guid HandlerId { get; set; }
        public string HandlerName { get; set; } = string.Empty; 
        public DocumentStatus OldStatus { get; set; }
        public DocumentStatus NewStatus { get; set; }
        public Guid? AppraisalAssignmentId { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<FeedbackIssueResponseDto> LinkedIssues { get; set; } = new();
    }

    public class AppraisalRejectDto
    {
        [Required]
        public Guid DocumentId { get; set; }

        public Guid? AppraisalAssignmentId { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        public List<FeedbackIssueCreateDto> NewIssues { get; set; } = new();
        public List<Guid?> AttachmentIds { get; set; } = new();
    }
}