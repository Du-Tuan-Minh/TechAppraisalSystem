using Application.Common;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;

namespace Application.Services
{
    public class AppraisalHistoryService : BaseService, IAppraisalHistoryService
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public AppraisalHistoryService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService) : base(unitOfWork)
        {
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<PagedResult<AppraisalHistoryResponseDto>>> GetDocumentAuditTrailAsync(Guid documentId, PaginationDto pagination)
        {
            var pagedHistory = await _unitOfWork.AppraisalHistory.GetHistoryByDocumentIdPagedAsync(documentId, pagination);
            var dtos = _mapper.Map<List<AppraisalHistoryResponseDto>>(pagedHistory.Items);

            var result = new PagedResult<AppraisalHistoryResponseDto>
            {
                Items = dtos,
                TotalCount = pagedHistory.TotalCount,
                Page = pagedHistory.Page,
                PageSize = pagedHistory.PageSize
            };

            return ApiResponse<PagedResult<AppraisalHistoryResponseDto>>.Success(result);
        }

        public async Task<ApiResponse<AppraisalHistoryResponseDto>> GetHistoryByVersionAsync(Guid versionId)
        {
            var history = await _unitOfWork.AppraisalHistory.GetHistoryByVersionId(versionId);
            if (history == null) return ApiResponse<AppraisalHistoryResponseDto>.Failure(404, "There is no appraisal history available for this version.");

            var dto = _mapper.Map<AppraisalHistoryResponseDto>(history);
            var issues = _unitOfWork.FeedbackIssues.GetByVersionIdAsync(versionId);
            dto.LinkedIssues = _mapper.Map<List<FeedbackIssueResponseDto>>(issues);

            return ApiResponse<AppraisalHistoryResponseDto>.Success(dto);
        }
    }
}