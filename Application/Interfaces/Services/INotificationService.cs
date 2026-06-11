using Application.Common;
using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<ApiResponse<PagedResult<UserNotificationResponseDto>>> GetMyNotificationsAsync(Guid userId, NotificationQueryDto query);
        Task<ApiResponse<bool>> MarkAsReadAsync(Guid userNotificationId, Guid userId);
        Task<ApiResponse<bool>> DeleteNotificationAsync(Guid userNotificationId, Guid userId);
        Task<ApiResponse<Guid>> CreateNotificationAsync(NotificationCreateDto dto);
        Task SendSystemNotificationAsync(Guid targetUserId, string title, string content, string? dataJson = null);
        Task SendNotificationWithRouteAsync(NotificationRouteType routeType, Guid targetUserId, string title, string content, Guid documentId, Guid? assignmentId = null, Guid? versionId = null, Guid? reviewerId = null, Guid? departmentId = null);
    }
}