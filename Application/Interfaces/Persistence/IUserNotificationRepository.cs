using Application.Common;
using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IUserNotificationRepository : IRepository<UserNotification>
    {
        Task<PagedResult<UserNotification>> GetNotificationsByUserIdAsync(Guid userId, NotificationQueryDto query);
    }
}