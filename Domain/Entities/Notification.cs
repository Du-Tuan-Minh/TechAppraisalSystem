using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Notifications")]
    public class Notification : BaseEntity
    {
        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; } 

        [Column(TypeName = "jsonb")]
        public string? Metadata { get; set; }

        public Guid? SenderId { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual User? Sender { get; set; }

        public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
    }
}
