using Application.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UserNotificationResponseDto
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public object? Metadata { get; set; }
        public Guid? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAvatar { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationCreateDto
    {
        [Required(ErrorMessage = "The announcement title is required.")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "The notification content is mandatory.")]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; } = NotificationType.System;

        public object? Metadata { get; set; }
        public Guid? SenderId { get; set; }

        [Required(ErrorMessage = "The recipient list must not be empty.")]
        [MinLength(1, ErrorMessage = "You must send a notification to at least one person.")]
        public List<Guid> TargetUserIds { get; set; } = new();
    }

    public class NotificationQueryDto : PaginationDto
    {
        public string? Search { get; set; }

        public NotificationType? Type { get; set; }

        public bool? IsRead { get; set; }

        public string SortBy { get; set; } = "createdAt";

        public string SortOrder { get; set; } = "desc";
    }
}