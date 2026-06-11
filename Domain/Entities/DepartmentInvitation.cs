using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("DepartmentInvitations")]
    public class DepartmentInvitation : BaseEntity
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required, MaxLength(255)]
        public string InviteeEmployeeCode { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string InvitationCode { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; } = null!;
    }
}