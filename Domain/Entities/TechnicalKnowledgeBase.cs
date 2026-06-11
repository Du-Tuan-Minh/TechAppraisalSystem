using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("TechnicalKnowledgeBase")]
    public class TechnicalKnowledgeBase : BaseEntity
    {
        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "jsonb")]
        public string LinkedSpecPattern { get; set; } = "{}";

        [Required]
        public string TechnicalSolution { get; set; } = string.Empty;

        [Required]
        public IssueSeverity Severity { get; set; } = IssueSeverity.Critical;

        public int OccurrenceCount { get; set; } = 1;

        public bool IsVerified { get; set; } = false;
        public Guid? VerifiedById { get; set; }
        public DateTime? LastVerifiedAt { get; set; }

        [ForeignKey(nameof(VerifiedById))]
        public virtual User? Verifier { get; set; }
    }
}
