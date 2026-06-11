using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Attachments")]
    public class Attachment : BaseEntity
    {
        [Required]
        public Guid TechnicalDocumentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public byte[] FileData { get; set; } = null!;

        public long FileSize { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        public AttachmentCategory ContentCategory { get; set; }

        [Required]
        public Guid UploadedById { get; set; }

        [ForeignKey(nameof(TechnicalDocumentId))]
        public virtual TechnicalDocument TechnicalDocument { get; set; } = null!;

        [ForeignKey(nameof(UploadedById))]
        public virtual User Uploader { get; set; } = null!;

        public virtual ICollection<AttachmentLink> Links { get; set; } = new List<AttachmentLink>();
    }
}