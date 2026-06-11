using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class AttachmentResponseDto
    {
        public Guid Id { get; set; }
        public Guid TechnicalDocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public AttachmentCategory ContentCategory { get; set; }
        public Guid UploadedById { get; set; }
        public string UploaderName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<AttachmentLinkDto> Links { get; set; } = new();
    }

    public class AttachmentLinkDto
    {
        public Guid EntityId { get; set; }
        public LinkedEntityType EntityType { get; set; }
    }

    public class AttachmentCreateDto
    {
        [Required]
        public Guid TechnicalDocumentId { get; set; }

        [Required]
        public AttachmentCategory ContentCategory { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn file.")]
        public IFormFile File { get; set; } = null!;
        public Guid? LinkedEntityId { get; set; }
        public LinkedEntityType? LinkedEntityType { get; set; }
    }

    public class CreateAttachmentLinkRequest
    {
        [Required]
        public Guid AttachmentId { get; set; }
        [Required]
        public Guid EntityId { get; set; }
        [Required]
        public LinkedEntityType EntityType { get; set; }
    }
}