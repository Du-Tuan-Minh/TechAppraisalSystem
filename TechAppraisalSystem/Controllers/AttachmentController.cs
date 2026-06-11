using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/attachments")]
    public class AttachmentController : BaseApiController<AttachmentController>
    {
        public AttachmentController(IServiceManager services, ILogger<AttachmentController> logger)
         : base(services, logger)
        { }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] AttachmentCreateDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.AttachmentService.UploadAttachmentAsync(request, userId);
            return StatusCode(result.StatusCode, result);
        }

        //[HttpGet("attachment-detail/{entityId}")]
        //public async Task<IActionResult> GetAttachmentDetail(Guid entityId, [FromQuery] AttachmentCategory? category)
        //{
        //    var result = await _serviceManager.AttachmentService.GetAttachmentsByEntityAsync(entityId, category);
        //    return StatusCode(result.StatusCode, result);
        //}

        [Authorize(Roles = nameof(UserRole.Staff))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.AttachmentService.DeleteAttachmentAsync(id, userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("attachment-file-detail/{id}")]
        public async Task<IActionResult> GetFileDetail(Guid id)
        {
            var result = await _serviceManager.AttachmentService.GetFileStreamAsync(id);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, result);

            return File(result.Data.Stream, result.Data.FileType, result.Data.FileName);
        }
    }
}