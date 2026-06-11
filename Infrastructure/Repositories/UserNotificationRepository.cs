using Application.Common;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class UserNotificationRepository : BaseRepository<UserNotification>, IUserNotificationRepository
{
    public UserNotificationRepository(ApplicationDbContext context, ILogger<UserNotificationRepository> logger)
             : base(context, logger)
    { }

    public async Task<PagedResult<UserNotification>> GetNotificationsByUserIdAsync(Guid userId, NotificationQueryDto query)
    {
        var notificationsQuery = _dbSet
            .AsNoTracking()
            .Where(un => un.UserId == userId)
            .Include(un => un.Notification)
                .ThenInclude(n => n.Sender)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();

            notificationsQuery = notificationsQuery.Where(un =>
                un.Notification.Title.ToLower().Contains(search) ||
                un.Notification.Content.ToLower().Contains(search));
        }

        if (query.Type.HasValue)
        {
            notificationsQuery = notificationsQuery.Where(un =>
               un.Notification.Type == query.Type.Value);
        }

        if (query.IsRead.HasValue)
        {
            notificationsQuery = notificationsQuery.Where(un =>
                un.IsRead == query.IsRead.Value);
        }

        notificationsQuery = (query.SortBy.ToLower(), query.SortOrder.ToLower()) switch
        {
            ("title", "asc") => notificationsQuery.OrderBy(un => un.Notification.Title),
            ("title", "desc") => notificationsQuery.OrderByDescending(un => un.Notification.Title),

            ("createdat", "asc") => notificationsQuery.OrderBy(un => un.CreatedAt),

            _ => notificationsQuery.OrderByDescending(un => un.CreatedAt)
        };

        return await notificationsQuery.ToPagedListAsync(query.Page, query.PageSize);
    }
}
