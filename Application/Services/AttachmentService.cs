using Application.Common;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class AttachmentService : BaseService, IAttachmentService
    {
        private readonly IMapper _mapper;
        private readonly string _storagePath;

        public AttachmentService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "InternalStorage", "Attachments");
            if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
        }

        public async Task<ApiResponse<AttachmentResponseDto>> UploadAttachmentAsync(AttachmentCreateDto dto, Guid userId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<AttachmentResponseDto>>(async () =>
            {
                if (dto.File == null || dto.File.Length == 0)
                    return ApiResponse<AttachmentResponseDto>.Failure(400, "File không hợp lệ.");

                if (dto.File.Length > 100 * 1024 * 1024)
                    return ApiResponse<AttachmentResponseDto>.Failure(400, "Dung lượng file vượt quá 100MB.");

                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await dto.File.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                var attachment = _mapper.Map<Attachment>(dto);
                attachment.FileName = dto.File.FileName;
                attachment.FileData = fileData;
                attachment.FileSize = dto.File.Length;
                attachment.FileType = dto.File.ContentType;
                attachment.UploadedById = userId;

                if (dto.LinkedEntityId.HasValue && dto.LinkedEntityType.HasValue)
                {
                    attachment.Links.Add(new AttachmentLink
                    {
                        EntityId = dto.LinkedEntityId.Value,
                        EntityType = dto.LinkedEntityType.Value
                    });
                }

                await _unitOfWork.Attachments.AddAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                var result = await _unitOfWork.Attachments.GetByIdAsync(attachment.Id, a => a.Uploader.Profile!);
                return ApiResponse<AttachmentResponseDto>.Success(_mapper.Map<AttachmentResponseDto>(result), "Tải file lên cơ sở dữ liệu thành công.");
            });
        }

        //public async Task<ApiResponse<IEnumerable<AttachmentResponseDto>>> GetAttachmentsByEntityAsync(Guid entityId, AttachmentCategory? category)
        //{
        //    var links = await _unitOfWork.AttachmentLinks.FindAsync(
        //        l => l.EntityId == entityId && (!category.HasValue || l.Attachment.ContentCategory == category.Value),
        //        l => l.Attachment,
        //        l => l.Attachment.Uploader.Profile!
        //    );

        //    var attachments = links.Select(l => l.Attachment).Distinct().ToList();
        //    var dtos = _mapper.Map<IEnumerable<AttachmentResponseDto>>(attachments);

        //    return ApiResponse<IEnumerable<AttachmentResponseDto>>.Success(dtos);
        //}

        public async Task<ApiResponse<bool>> DeleteAttachmentAsync(Guid attachmentId, Guid userId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var attachment = await _unitOfWork.Attachments.GetByIdAsync(attachmentId);

                if (attachment == null)
                    return ApiResponse<bool>.Failure(404, "Không tìm thấy file.");

                if (attachment.UploadedById != userId)
                    return ApiResponse<bool>.Failure(403, "Bạn không có quyền xóa file này.");

                _unitOfWork.Attachments.Remove(attachment);

                var result = await _unitOfWork.SaveChangesAsync() > 0;

                return result
                    ? ApiResponse<bool>.Success(true, "Xóa file thành công.")
                    : ApiResponse<bool>.Failure(500, "Lỗi trong quá trình xóa dữ liệu.");
            });
        }

        public async Task<ApiResponse<(Stream Stream, string FileType, string FileName)>> GetFileStreamAsync(Guid attachmentId)
        {
            var attachment = await _unitOfWork.Attachments.GetByIdAsync(attachmentId);

            if (attachment == null)
                return ApiResponse<(Stream, string, string)>.Failure(404, "Không tìm thấy tài liệu đính kèm.");

            if (attachment.FileData == null || attachment.FileData.Length == 0)
                return ApiResponse<(Stream, string, string)>.Failure(404, "Nội dung file trống hoặc bị lỗi.");

            // Tạo stream từ byte array
            var stream = new MemoryStream(attachment.FileData);

            // Senior tip: Luôn đảm bảo Position ở 0 trước khi trả về
            stream.Position = 0;

            return ApiResponse<(Stream, string, string)>.Success((stream, attachment.FileType, attachment.FileName));
        }
    }
}