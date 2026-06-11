using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Profiles")]
    public class Profile
    {
        [Key, ForeignKey(nameof(UserId))]
        public Guid UserId { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(255)]
        public string? FirstName { get; set; }

        [MaxLength(255)]
        public string? LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public virtual User User { get; set; } = null!;
    }
}
