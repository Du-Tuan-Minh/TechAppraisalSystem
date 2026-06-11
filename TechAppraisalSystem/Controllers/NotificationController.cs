using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : BaseApiController<NotificationController>
    {
        public NotificationController(IServiceManager services, ILogger<NotificationController> logger)
       : base(services, logger)
        { }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationQueryDto query)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.NotificationService.GetMyNotificationsAsync(userId, query);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.NotificationService.MarkAsReadAsync(id, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.NotificationService.DeleteNotificationAsync(id, userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}