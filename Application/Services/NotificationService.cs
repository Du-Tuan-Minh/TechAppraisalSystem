using Application.Common;
using Application.DTOs;
using Application.Hubs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Application.Services
{
    public class NotificationService : BaseService, INotificationService
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext) : base(unitOfWork)
        {
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<ApiResponse<PagedResult<UserNotificationResponseDto>>> GetMyNotificationsAsync(Guid userId, NotificationQueryDto query)
        {
            var pagedNotifications = await _unitOfWork.UserNotifications
                .GetNotificationsByUserIdAsync(userId, query);

            return ApiResponse<PagedResult<UserNotificationResponseDto>>.Success(
                new PagedResult<UserNotificationResponseDto>
                {
                    Items = _mapper.Map<List<UserNotificationResponseDto>>(pagedNotifications.Items),
                    TotalCount = pagedNotifications.TotalCount,
                    Page = pagedNotifications.Page,
                    PageSize = pagedNotifications.PageSize
                });
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid userNotificationId, Guid userId)
        {
            var userNoti = await _unitOfWork.UserNotifications.GetByIdAsync(
                userNotificationId,
                un => un.Notification,
                un => un.Notification.Sender);

            if (userNoti == null || userNoti.UserId != userId)
                return ApiResponse<bool>.Failure(404, "Notification not found.");

            if (!userNoti.IsRead)
            {
                userNoti.IsRead = true;
                userNoti.ReadAt = DateTime.UtcNow;

                _unitOfWork.UserNotifications.Update(userNoti);
                await _unitOfWork.SaveChangesAsync();

                await _hubContext.Clients.Group($"user-{userId}")
                     .SendAsync("NotificationRead", userNotificationId);
            }

            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<bool>> DeleteNotificationAsync(Guid userNotificationId, Guid userId)
        {
            var userNoti = await _unitOfWork.UserNotifications.GetByIdAsync(userNotificationId);

            if (userNoti == null || userNoti.UserId != userId)
                return ApiResponse<bool>.Failure(404, "Notification not found.");

            _unitOfWork.UserNotifications.Remove(userNoti);

            await _unitOfWork.SaveChangesAsync();

            await _hubContext.Clients.Group($"user-{userId}")
                 .SendAsync("NotificationDeleted", userNotificationId);

            return ApiResponse<bool>.Success(true, "The notification has been deleted.");
        }

        public async Task<ApiResponse<Guid>> CreateNotificationAsync(NotificationCreateDto dto)
        {
            var notification = _mapper.Map<Notification>(dto);

            await _unitOfWork.Notifications.AddAsync(notification);

            var userNotifications = dto.TargetUserIds.Select(userId => new UserNotification
            {
                UserId = userId,
                Notification = notification,
                IsRead = false
            }).ToList();

            await _unitOfWork.UserNotifications.AddRangeAsync(userNotifications);

            await _unitOfWork.SaveChangesAsync();

            var sendTasks = userNotifications.Select(async userNoti =>
            {
                var responseDto = new UserNotificationResponseDto
                {
                    Id = userNoti.Id,
                    NotificationId = notification.Id,
                    Title = notification.Title,
                    Content = notification.Content,
                    Type = notification.Type,
                    Metadata = string.IsNullOrWhiteSpace(notification.Metadata)
                        ? null
                        : System.Text.Json.JsonSerializer.Deserialize<object>(notification.Metadata),
                    SenderId = notification.SenderId,
                    SenderName = null,
                    SenderAvatar = null,
                    IsRead = userNoti.IsRead,
                    ReadAt = userNoti.ReadAt,
                    CreatedAt = userNoti.CreatedAt
                };

                try
                {
                    await _hubContext.Clients
                        .Group($"user-{userNoti.UserId}")
                        .SendAsync("ReceiveNotification", responseDto);
                }
                catch
                {
                    // optional: log error
                }
            });

            await Task.WhenAll(sendTasks);

            return ApiResponse<Guid>.Success(notification.Id, "Notification sent successfully.");
        }

        public async Task SendSystemNotificationAsync(Guid targetUserId, string title, string content, string? dataJson = null)
        {
            await CreateNotificationAsync(new NotificationCreateDto
            {
                Title = title,
                Content = content,
                Type = NotificationType.System,
                Metadata = dataJson,
                SenderId = null,
                TargetUserIds = new List<Guid> { targetUserId }
            });
        }

        public async Task SendNotificationWithRouteAsync(NotificationRouteType routeType, Guid targetUserId, string title, string content, Guid documentId, Guid? assignmentId = null, Guid? versionId = null, Guid? reviewerId = null, Guid? departmentId = null)
        {
            await CreateNotificationAsync(new NotificationCreateDto
            {
                Title = title,
                Content = content,
                Type = NotificationType.Assignment,
                SenderId = null,
                TargetUserIds = new List<Guid> { targetUserId },
                Metadata = JsonSerializer.Serialize(new
                {
                    routeType = (int)routeType,
                    documentId,
                    assignmentId,
                    versionId,
                    reviewerId,
                    departmentId
                })
            });
        }
    }
}