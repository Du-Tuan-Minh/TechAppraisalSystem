using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class FeedbackCommentDto
    {
        public Guid Id { get; set; }
        public Guid FeedbackIssueId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Danh sách các phản hồi con (Chỉ lấy 1 cấp)
        public List<FeedbackCommentDto> Replies { get; set; } = new();

        // Danh sách file đính kèm nếu có
        public List<AttachmentResponseDto> Attachments { get; set; } = new();
    }

    public class CreateFeedbackCommentRequest
    {
        [Required]
        public Guid FeedbackIssueId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        // Nếu là comment gốc thì để null, nếu là reply thì truyền Id của comment cha
        public Guid? ParentCommentId { get; set; }

        // Danh sách Id các file đã upload trước đó muốn gắn vào comment
        public List<Guid>? AttachmentIds { get; set; }
    }
}
