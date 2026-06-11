using Application.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class TechnicalKnowledgeBaseResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public IssueSeverity Severity { get; set; }
        public int OccurrenceCount { get; set; }
        public bool IsVerified { get; set; }
    }

    public class TechnicalKnowledgeBaseDetailDto: TechnicalKnowledgeBaseResponseDto
    {

        public object LinkedSpecPattern { get; set; } = new();
        public string TechnicalSolution { get; set; } = string.Empty;
        public Guid? VerifiedById { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? LastVerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TechnicalKnowledgeBaseCreateDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public object LinkedSpecPattern { get; set; } = new();

        [Required(ErrorMessage = "A technical solution is required.")]
        public string TechnicalSolution { get; set; } = string.Empty;

        [Required]
        public IssueSeverity Severity { get; set; } = IssueSeverity.Minor;
    }

    public class TechnicalKnowledgeBaseUpdateDto
    {
        [MaxLength(255)]
        public string? Title { get; set; }

        public object? LinkedSpecPattern { get; set; }
        public string? TechnicalSolution { get; set; }
        public IssueSeverity? Severity { get; set; }
        public bool? IsVerified { get; set; }
    }

    public class KnowledgeBaseFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public IssueSeverity? Severity { get; set; }
        public bool? IsVerified { get; set; }
    }

    public class SuggestionResponseDto
    {
        public string SourceTitle { get; set; } = string.Empty;
        public object? LinkedSpecPattern { get; set; }
        public List<AttachmentShortDto> Attachments { get; set; } = new();
    }

    public class AttachmentShortDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public AttachmentCategory ContentCategory { get; set; }
    }
}