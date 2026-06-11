using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("FeedbackComments")]
    public class FeedbackComment : BaseEntity
    {
        [Required]
        public Guid FeedbackIssueId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(2000)] 
        public string Content { get; set; } = string.Empty;

        public Guid? ParentCommentId { get; set; }

        [ForeignKey(nameof(FeedbackIssueId))]
        public virtual FeedbackIssue FeedbackIssue { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(ParentCommentId))]
        public virtual FeedbackComment? ParentComment { get; set; }

        [InverseProperty(nameof(ParentComment))]
        public virtual ICollection<FeedbackComment> Replies { get; set; } = new List<FeedbackComment>();

        public virtual ICollection<AttachmentLink> AttachmentLinks { get; set; } = new List<AttachmentLink>();
    }
}