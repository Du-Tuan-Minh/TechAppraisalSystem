using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("AttachmentLinks")]
    public class AttachmentLink : BaseEntity
    {
        [Required]
        public Guid AttachmentId { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public LinkedEntityType EntityType { get; set; }

        [ForeignKey(nameof(AttachmentId))]
        public virtual Attachment Attachment { get; set; } = null!;
    }
}