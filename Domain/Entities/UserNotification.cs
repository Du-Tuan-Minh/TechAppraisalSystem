using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("UserNotifications")]
    public class UserNotification : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid NotificationId { get; set; }

        [Required]
        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(NotificationId))]
        public virtual Notification Notification { get; set; } = null!;
    }
}