using Application.Common;
using Application.DTOs;
using Application.Extensions;
using Application.Hubs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Application.Services
{
    public class FeedbackCommentService : BaseService, IFeedbackCommentService
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public FeedbackCommentService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> hubContext) : base(unitOfWork)
        {
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<ApiResponse<FeedbackCommentDto>> CreateCommentAsync(Guid userId, CreateFeedbackCommentRequest request)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<FeedbackCommentDto>>(async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null) return ApiResponse<FeedbackCommentDto>.Failure(404, "Người dùng không tồn tại.");

                var issueExists = await _unitOfWork.FeedbackIssues.AnyAsync(i => i.Id == request.FeedbackIssueId);
                if (!issueExists) return ApiResponse<FeedbackCommentDto>.Failure(404, "Feedback issue không tồn tại.");

                var comment = _mapper.Map<FeedbackComment>(request);
                comment.UserId = userId;

                if (request.AttachmentIds?.Any() == true)
                {
                    foreach (var fileId in request.AttachmentIds)
                    {
                        comment.AttachmentLinks.Add(new AttachmentLink
                        {
                            AttachmentId = fileId,
                            EntityType = LinkedEntityType.FeedbackComment
                        });
                    }
                }

                await _unitOfWork.FeedbackComments.AddAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                var result = await _unitOfWork.FeedbackComments.GetCommentWithDetailsAsync(comment.Id);

                var dto = _mapper.Map<FeedbackCommentDto>(result);

                await _hubContext.Clients
                    .Group($"issue-{request.FeedbackIssueId}")
                    .SendAsync("ReceiveComment", dto);

                return ApiResponse<FeedbackCommentDto>.Success(dto, "Tạo comment thành công.");
            });
        }

        public async Task<ApiResponse<PagedResult<FeedbackCommentDto>>> GetCommentsByIssueAsync(Guid issueId, PaginationDto pagination)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<PagedResult<FeedbackCommentDto>>>(async () =>
            {
                var issueExists = await _unitOfWork.FeedbackIssues.AnyAsync(i => i.Id == issueId);
                if (!issueExists) return ApiResponse<PagedResult<FeedbackCommentDto>>.Failure(404, "Feedback issue không tồn tại.");

                var query = _unitOfWork.FeedbackComments.GetCommentsByIssueWithDetailsQuery(issueId);

                var pagedEntities = await query.ToPagedListAsync(pagination.Page, pagination.PageSize);

                var result = new PagedResult<FeedbackCommentDto>
                {
                    Items = _mapper.Map<List<FeedbackCommentDto>>(pagedEntities.Items),
                    TotalCount = pagedEntities.TotalCount,
                    Page = pagedEntities.Page,
                    PageSize = pagedEntities.PageSize
                };

                return ApiResponse<PagedResult<FeedbackCommentDto>>.Success(result);
            });
        }
    }
}