using Application.Common;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class FeedbackService : BaseService, IFeedbackService
    {
        private readonly IMapper _mapper;

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<FeedbackIssueResponseDto>>> GetIssuesByDocumentIdAsync(Guid documentId, PaginationDto pagination)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<PagedResult<FeedbackIssueResponseDto>>>(async () =>
            {
                var query = _unitOfWork.FeedbackIssues.GetFeedbackByDocumentIdAsync(documentId);
                var pagedIssues = await query.ToPagedListAsync(pagination.Page, pagination.PageSize);
                var dtos = _mapper.Map<List<FeedbackIssueResponseDto>>(pagedIssues.Items);

                return ApiResponse<PagedResult<FeedbackIssueResponseDto>>.Success(new PagedResult<FeedbackIssueResponseDto>
                {
                    Items = dtos,
                    TotalCount = pagedIssues.TotalCount,
                    Page = pagedIssues.Page,
                    PageSize = pagedIssues.PageSize
                }, "Lấy danh sách lỗi thành công");
            });
        }

        public async Task<ApiResponse<PagedResult<FeedbackIssueResponseDto>>> GetIssuesByVersionIdAsync(Guid versionId, PaginationDto pagination)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<PagedResult<FeedbackIssueResponseDto>>>(async () =>
            {
                var query = _unitOfWork.FeedbackIssues.GetByVersionIdAsync(versionId);
                var pagedIssues = await query.ToPagedListAsync(pagination.Page, pagination.PageSize);
                var dtos = _mapper.Map<List<FeedbackIssueResponseDto>>(pagedIssues.Items);

                return ApiResponse<PagedResult<FeedbackIssueResponseDto>>.Success(new PagedResult<FeedbackIssueResponseDto>
                {
                    Items = dtos,
                    TotalCount = pagedIssues.TotalCount,
                    Page = pagedIssues.Page,
                    PageSize = pagedIssues.PageSize
                });
            });
        }

        public async Task<ApiResponse<FeedbackIssueDetailDto>> GetIssueByIdAsync(Guid issueId)
        {
            var issue = await _unitOfWork.FeedbackIssues.GetDetailWithReporterAsync(issueId);
            if (issue == null) return ApiResponse<FeedbackIssueDetailDto>.Failure(404, "Không tìm thấy lỗi.");

            var dto = _mapper.Map<FeedbackIssueDetailDto>(issue);

            return ApiResponse<FeedbackIssueDetailDto>.Success(dto);
        }

        //public async Task<ApiResponse<FeedbackIssueResponseDto>> AddReviewIssueAsync(FeedbackIssueCreateDto dto, Guid staffId)
        //{
        //    var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(dto.DocumentId, d => d.Versions);
        //    if (doc == null) return ApiResponse<FeedbackIssueResponseDto>.Failure(404, "Tài liệu không tồn tại.");

        //    var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);
        //    if (currentVersion == null) return ApiResponse<FeedbackIssueResponseDto>.Failure(400, "Không có phiên bản hiện hành.");

        //    var issue = _mapper.Map<FeedbackIssue>(dto);
        //    issue.ReporterId = staffId;
        //    issue.RequestVersionId = currentVersion.Id;
        //    issue.Status = IssueStatus.New;

        //    await _unitOfWork.FeedbackIssues.AddAsync(issue);
        //    await _unitOfWork.SaveChangesAsync();

        //    return ApiResponse<FeedbackIssueResponseDto>.Success(_mapper.Map<FeedbackIssueResponseDto>(issue));
        //}

        public async Task<ApiResponse<List<FeedbackIssueResponseDto>>> AddReviewIssuesAsync(List<FeedbackIssueCreateDto> dtos, Guid staffId)
        {
            if (dtos == null || !dtos.Any()) return ApiResponse<List<FeedbackIssueResponseDto>>.Failure(400, "Danh sách rỗng");
            var documentId = dtos.First().DocumentId;
            var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(documentId, d => d.Versions);
            if (doc == null) return ApiResponse<List<FeedbackIssueResponseDto>>.Failure(404, "Tài liệu không tồn tại.");

            var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);
            if (currentVersion == null) return ApiResponse<List<FeedbackIssueResponseDto>>.Failure(400, "Không có phiên bản hiện hành.");

            var issues = dtos.Select(dto =>
            {
                var issue = _mapper.Map<FeedbackIssue>(dto);
                issue.ReporterId = staffId;
                issue.RequestVersionId = currentVersion.Id;
                issue.Status = IssueStatus.New;
                return issue;
            }).ToList();

            await _unitOfWork.FeedbackIssues.AddRangeAsync(issues);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<List<FeedbackIssueResponseDto>>.Success(_mapper.Map<List<FeedbackIssueResponseDto>>(issues));
        }

        public async Task<ApiResponse<bool>> UpdateIssueStatusAsync(Guid issueId, IssueStatus newStatus, string? note, IssueCategory? category, Guid staffId)
        {
            //var issue = await _unitOfWork.FeedbackIssues.GetDetailWithReporterAsync(issueId); 
            //if (issue == null) return ApiResponse<bool>.Failure(404, "Không tìm thấy lỗi.");
            //issue.Status = newStatus;

            //if (issue.Document != null)
            //{
            //    if (newStatus == IssueStatus.InProcessing && issue.Document.Status == DocumentStatus.FeedbackReceived)
            //    {
            //        issue.Document.Status = DocumentStatus.UnderImprovement;
            //        _unitOfWork.TechnicalDocument.Update(issue.Document);
            //    }
            //}

            //_unitOfWork.FeedbackIssues.Update(issue);
            var result = await _unitOfWork.SaveChangesAsync() > 0;

            return result
                ? ApiResponse<bool>.Success(true, "Cập nhật trạng thái thành công.")
                : ApiResponse<bool>.Failure(500, "Lỗi khi lưu dữ liệu.");
        }

        public async Task<ApiResponse<bool>> FinalizeIssueClosureAsync(Guid issueId, Guid specialistId)
        {
            //return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () => {
            //    var issue = await _unitOfWork.FeedbackIssues.GetByIdAsync(issueId, i => i.Document);
            //    if (issue == null) return ApiResponse<bool>.Failure(404, "No error found.");
            //    if (string.IsNullOrEmpty(issue.ResolutionNote)) return ApiResponse<bool>.Failure(400, "The bug cannot be closed without a technical solution note.");
            //    issue.Status = IssueStatus.Closed;

            //    var kbEntry = new TechnicalKnowledgeBase
            //    {
            //        Title = issue.IndicatorPath,
            //        LinkedSpecPattern = issue.IndicatorPath,
            //        TechnicalSolution = issue.ResolutionNote,
            //        Severity = issue.Severity,
            //        OccurrenceCount = 1,
            //        IsVerified = true,
            //        VerifiedById = specialistId,
            //        LastVerifiedAt = DateTime.UtcNow,
            //        CreatedAt = DateTime.UtcNow
            //    };

            //    await _unitOfWork.TechnicalKnowledgeBase.AddAsync(kbEntry);
            //    issue.TechnicalKnowledgeBaseId = kbEntry.Id;

            //    _unitOfWork.FeedbackIssues.Update(issue);
            //    await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "The knowledge has been transferred to the AI ​​for later verification.");
            //});
        }
    }
}