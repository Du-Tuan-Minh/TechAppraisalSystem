using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAttachmentService
    {
        Task<ApiResponse<AttachmentResponseDto>> UploadAttachmentAsync(AttachmentCreateDto dto, Guid userId);
        //Task<ApiResponse<IEnumerable<AttachmentResponseDto>>> GetAttachmentsByEntityAsync(Guid entityId, AttachmentCategory? category);
        Task<ApiResponse<bool>> DeleteAttachmentAsync(Guid attachmentId, Guid userId);
        Task<ApiResponse<(Stream Stream, string FileType, string FileName)>> GetFileStreamAsync(Guid attachmentId);
    }
}