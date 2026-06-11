using Application.Common;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System.Text.Json;

namespace Application.Services
{
    public class TechnicalDocumentService : BaseService, ITechnicalDocumentService
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        public TechnicalDocumentService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService) : base(unitOfWork)
        {
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<TechnicalDocumentDetailDto>> CreateDocumentAsync(TechnicalDocumentCreateDto dto, Guid requesterId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<TechnicalDocumentDetailDto>>(async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(requesterId);
                if (user == null || user.DepartmentId == null) return ApiResponse<TechnicalDocumentDetailDto>.Failure(400, "Tài khoản chưa được gán đơn vị.");

                var doc = _mapper.Map<TechnicalDocument>(dto);
                doc.RequesterId = requesterId;
                doc.DepartmentId = user.DepartmentId.Value;

                var firstVersion = new RequestVersion
                {
                    Id = Guid.NewGuid(),
                    VersionNumber = 1,
                    TechnicalSpecsJson = JsonSerializer.Serialize(dto.TechnicalSpecs ?? new Dictionary<string, object>(),
                                         new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    ChangeReason = "Khởi tạo tài liệu lần đầu.",
                    IsCurrent = true,
                    CreatedAt = DateTime.UtcNow
                };

                doc.Versions.Add(firstVersion);

                //if (dto.AttachmentIds != null && dto.AttachmentIds.Any())
                //{
                //    var attachments = await _unitOfWork.Attachments.FindAsync(a => dto.AttachmentIds.Contains(a.Id));
                //    foreach (var att in attachments)
                //    {
                //        doc.Attachments.Add(att);

                //        att.Links.Add(new AttachmentLink
                //        {
                //            EntityId = firstVersion.Id,
                //            EntityType = LinkedEntityType.RequestVersion
                //        });
                //    }
                //}

                await _unitOfWork.TechnicalDocument.AddAsync(doc);
                await _unitOfWork.SaveChangesAsync();

                var resultDto = _mapper.Map<TechnicalDocumentDetailDto>(doc);
                return ApiResponse<TechnicalDocumentDetailDto>.Success(resultDto);
            });
        }

        public async Task<ApiResponse<TechnicalDocumentDetailDto>> GetDocumentDetailAsync(Guid id)
        {
            var doc = await _unitOfWork.TechnicalDocument.GetDetailDocumentAsync(id);

            return doc != null
                ? ApiResponse<TechnicalDocumentDetailDto>.Success(_mapper.Map<TechnicalDocumentDetailDto>(doc))
                : ApiResponse<TechnicalDocumentDetailDto>.Failure(404, "Not found");
        }

        public async Task<ApiResponse<bool>> SubmitInternalApprovalAsync(Guid documentId, Guid requesterId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(documentId, d => d.Versions);
                if (doc == null) return ApiResponse<bool>.Failure(404, "Tài liệu không tồn tại.");

                var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);
                if (currentVersion == null) return ApiResponse<bool>.Failure(400, "Tài liệu không có phiên bản hiện hành.");

                var user = await _unitOfWork.Users.GetByIdAsync(requesterId);
                if (user == null) return ApiResponse<bool>.Failure(400, "Bạn không có quyền nộp tài liệu này");

                bool isOwner = doc.RequesterId == requesterId;
                bool isHandler = doc.CurrentHandlerId == requesterId;
                var roleUser = user.Role;
                Guid nextHandlerId;
                DocumentStatus nextStatus;
                string comment;

                if (!isOwner && !isHandler) return ApiResponse<bool>.Failure(403, "Bạn không có quyền thực hiện hành động này.");

                if (roleUser == UserRole.Staff)
                {
                    if (doc.Status != DocumentStatus.Draft) return ApiResponse<bool>.Failure(400, "Tài liệu phải ở trạng thái Nháp mới có thể gửi.");
                    var unitLeader = await _unitOfWork.Users.GetUnitLeaderAsync(doc.DepartmentId);
                    if (unitLeader == null) return ApiResponse<bool>.Failure(404, "Không tìm thấy trưởng đơn vị để phê duyệt.");

                    nextHandlerId = unitLeader.Id;
                    nextStatus = DocumentStatus.InternalPending;
                    comment = $"Nhân viên '{user?.Profile?.FullName}' gửi trình ký nội bộ cho Trưởng đơn vị. Tài liệu: {doc.Title}.";
                }
                else if (roleUser == UserRole.Manager)
                {
                    var director = await _unitOfWork.Users.GetDirectorByDepartmentAsync(doc.DepartmentId);
                    if (director == null) return ApiResponse<bool>.Failure(404, "Không tìm thấy lãnh đạo trung tâm để phê duyệt.");

                    nextHandlerId = director.Id;
                    nextStatus = DocumentStatus.InternalApproved;
                    comment = $"Trưởng đơn vị: {user.Profile.FullName} của phòng ban: {user.Department.NameDepartment}  gửi trình ký cho Ban Giám đốc Trung tâm. Tài liệu: {doc.Title}.";
                }
                else
                {
                    return ApiResponse<bool>.Failure(403, "Vai trò của bạn không được phép khởi tạo luồng trình ký này.");
                }

                var oldStatus = doc.Status;
                doc.Status = nextStatus;
                doc.CurrentHandlerId = nextHandlerId;
                _unitOfWork.TechnicalDocument.Update(doc);

                var history = new AppraisalHistory
                {
                    DocumentId = documentId,
                    HandlerId = requesterId,
                    RequestVersionId = currentVersion.Id,
                    OldStatus = oldStatus,
                    NewStatus = doc.Status,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.AppraisalHistory.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                await _notificationService.SendNotificationWithRouteAsync(
                    NotificationRouteType.DocumentDetail,
                    nextHandlerId,
                    "Yêu cầu phê duyệt nội bộ",
                    comment,
                    doc.Id,
                    versionId: currentVersion.Id
                );

                return ApiResponse<bool>.Success(true, "The internal browser has been successfully submitted.");
            });
        }

        public async Task<ApiResponse<bool>> UpdateDraftAsync(Guid documentId, Guid userId, TechnicalDocumentUpdateDto dto)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(documentId, d => d.Versions);
                if (doc == null) return ApiResponse<bool>.Failure(404, "Tài liệu không tồn tại.");

                if (doc.RequesterId != userId) return ApiResponse<bool>.Failure(403, "Bạn không có quyền chỉnh sửa.");

                if (doc.Status != DocumentStatus.Draft &&
                    doc.Status != DocumentStatus.Rejected)
                {
                    return ApiResponse<bool>.Failure(400, "Chỉ được chỉnh sửa khi tài liệu ở bản thảo hoặc bị loại bỏ.");
                }

                var oldStatus = doc.Status;

                _mapper.Map(dto, doc);

                doc.ExternalDepartmentIds = JsonSerializer.Serialize(dto.ExternalDepartmentIds ?? new List<Guid>());

                doc.ApprovalProposerIds = JsonSerializer.Serialize(
                    dto.ApprovalProposerIds.HasValue
                        ? new List<Guid> { dto.ApprovalProposerIds.Value }
                        : new List<Guid>());

                var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);

                if (currentVersion == null) return ApiResponse<bool>.Failure(404, "Không tìm thấy phiên bản hiện tại.");

                // CASE 1: Draft
                if (doc.Status == DocumentStatus.Draft)
                {
                    _mapper.Map(dto, currentVersion);

                    if (dto.TechnicalSpecs != null)
                    {
                        currentVersion.TechnicalSpecsJson = JsonSerializer.Serialize(dto.TechnicalSpecs);
                    }

                    _unitOfWork.RequestVersions.Update(currentVersion);
                }

                // CASE 2: Rejected
                else if (doc.Status == DocumentStatus.Rejected)
                {
                    currentVersion.IsCurrent = false;

                    _unitOfWork.RequestVersions.Update(currentVersion);

                    var newVersion = new RequestVersion
                    {
                        Id = Guid.NewGuid(),
                        RequestId = doc.Id,
                        VersionNumber = currentVersion.VersionNumber + 1,
                        IsCurrent = true,
                        ChangeReason = dto.ChangeReason,
                        TechnicalSpecsJson = JsonSerializer.Serialize(
                            dto.TechnicalSpecs ?? new Dictionary<string, object>()),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.RequestVersions.AddAsync(newVersion);

                    doc.Status = DocumentStatus.Draft;
                    doc.CurrentHandlerId = userId;
                    doc.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.AppraisalHistory.AddAsync(
                        new AppraisalHistory
                        {
                            DocumentId = doc.Id,
                            HandlerId = userId,
                            RequestVersionId = newVersion.Id,
                            OldStatus = oldStatus,
                            NewStatus = DocumentStatus.Draft,
                            Comment = $"Tạo phiên bản {newVersion.VersionNumber}: {dto.ChangeReason}",
                            CreatedAt = DateTime.UtcNow
                        });
                }

                _unitOfWork.TechnicalDocument.Update(doc);

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Cập nhật tài liệu thành công.");
            });
        }

        public async Task<ApiResponse<PagedResult<TechnicalDocumentResponseDto>>> GetDocumentsPagedAsync(DocumentFilterDto filter, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return ApiResponse<PagedResult<TechnicalDocumentResponseDto>>.Failure(401, "The user does not exist.");

            var pagedDocs = await _unitOfWork.TechnicalDocument.GetDocumentsFilteredAsync(filter, userId, user.Role, user.DepartmentId);

            var response = _mapper.Map<PagedResult<TechnicalDocumentResponseDto>>(pagedDocs);
            return ApiResponse<PagedResult<TechnicalDocumentResponseDto>>.Success(response);
        }

        public async Task<ApiResponse<IEnumerable<DocumentVersionDto>>> GetDocumentVersionsAsync(Guid documentId, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(documentId, d => d.Versions);
            if (doc == null) return ApiResponse<IEnumerable<DocumentVersionDto>>.Failure(404, "The document does not exist.");

            if (user.Role == UserRole.Staff || user.Role == UserRole.Manager)
            {
                if (doc.DepartmentId != user.DepartmentId) return ApiResponse<IEnumerable<DocumentVersionDto>>.Failure(403, "You do not have permission to view the version of this document.");
            }

            if (user.Role == UserRole.Admin) return ApiResponse<IEnumerable<DocumentVersionDto>>.Failure(403, "Admin do not have access to technical content.");

            var versions = doc.Versions.OrderByDescending(v => v.VersionNumber);
            return ApiResponse<IEnumerable<DocumentVersionDto>>.Success(_mapper.Map<IEnumerable<DocumentVersionDto>>(versions));
        }

        public async Task<ApiResponse<PagedResult<TechnicalDocumentResponseDto>>> GetMyTasksPagedAsync(Guid userId, PaginationDto pagination)
        {
            var pagedTasks = await _unitOfWork.TechnicalDocument.GetMyTasksAsync(userId, pagination);
            var response = _mapper.Map<PagedResult<TechnicalDocumentResponseDto>>(pagedTasks);

            for (int i = 0; i < pagedTasks.Items.Count; i++)
            {
                var sourceDoc = pagedTasks.Items[i];
                var dto = response.Items[i];

                var currentAssignment = sourceDoc.AppraisalAssignments?
                    .FirstOrDefault(a => (a.ResponsibleManagerId == userId || a.Reviewers.Any(r => r.StaffId == userId))
                                         && a.Status != AssignmentStatus.Completed);

                dto.CurrentAssignmentId = currentAssignment?.Id;
                dto.CurrentReviewerId = currentAssignment?.Reviewers?.FirstOrDefault(r => r.StaffId == userId)?.Id;
                dto.CurrentVersionId = sourceDoc.Versions?.FirstOrDefault(v => v.IsCurrent)?.Id;
            }

            return ApiResponse<PagedResult<TechnicalDocumentResponseDto>>.Success(response, "Success");
        }

        public async Task<ApiResponse<DocumentVersionDetailDto>> GetVersionDetailAsync(Guid versionId, Guid userId)
        {
            var doc = await _unitOfWork.TechnicalDocument.GetVersionWithReviewersAsync(versionId);
            if (doc == null) return ApiResponse<DocumentVersionDetailDto>.Failure(404, "Tài liệu hoặc phiên bản không tồn tại.");

            var version = doc?.Versions.FirstOrDefault(v => v.Id == versionId);
            if (version == null) return ApiResponse<DocumentVersionDetailDto>.Failure(404, "Phiên bản không tồn tại.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return ApiResponse<DocumentVersionDetailDto>.Failure(401, "Người dùng không tồn tại.");

            bool isOwner = doc.RequesterId == userId;
            bool isCurrentHandler = doc.CurrentHandlerId == userId;
            bool isAssigned = version.AppraisalAssignments.Any(a =>
                 a.ResponsibleManagerId == userId || a.Reviewers.Any(r => r.StaffId == userId));
            bool isGlobalAuditor = user.Role == UserRole.Inspector || user.Role == UserRole.Manager;

            if (!isOwner && !isCurrentHandler && !isAssigned && !isGlobalAuditor)
            {
                return ApiResponse<DocumentVersionDetailDto>.Failure(403, "Bạn không có quyền truy cập vào nội dung kỹ thuật của phiên bản này.");
            }

            if (user.Role == UserRole.Admin)
                return ApiResponse<DocumentVersionDetailDto>.Failure(403, "Quản trị viên chỉ quản lý hệ thống, không có quyền xem nội dung chuyên môn.");

            return ApiResponse<DocumentVersionDetailDto>.Success(_mapper.Map<DocumentVersionDetailDto>(version));
        }

        public async Task<ApiResponse<PagedResult<UserCurrentDocumentDto>>> GetMyCurrentDocumentsAsync(Guid userId, UserCurrentDocumentFilterDto filter)
        {
            var result = await _unitOfWork.TechnicalDocument.GetMyCurrentDocumentsAsync(userId, filter);
            var response = _mapper.Map<PagedResult<UserCurrentDocumentDto>>(result);

            return ApiResponse<PagedResult<UserCurrentDocumentDto>>.Success(response);
        }

        public async Task<ApiResponse<PagedResult<ManagerDashboardDocumentDto>>> GetManagerStatusDocumentsAsync(Guid managerId, ManagerDashboardDocumentFilterDto filter)
        {
            var result = await _unitOfWork.TechnicalDocument.GetManagerStatusDocumentsAsync(managerId, filter);
            var mapped = _mapper.Map<PagedResult<ManagerDashboardDocumentDto>>(result);

            return ApiResponse<PagedResult<ManagerDashboardDocumentDto>>.Success(mapped);
        }

        //public async Task<ApiResponse<PagedResult<PendingAppraisalResponseDto>>> GetPendingAppraisalResponsesAsync(Guid managerId, PendingAppraisalFilterDto filter)
        //{
        //    return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
        //    {
        //        var result = await _unitOfWork.TechnicalDocument.GetPendingAppraisalResponsesAsync(managerId, filter);
        //        var response = new PagedResult<PendingAppraisalResponseDto>
        //        {
        //            Items = _mapper.Map<List<PendingAppraisalResponseDto>>(result.Items),
        //            TotalCount = result.TotalCount,
        //            Page = result.Page,
        //            PageSize = result.PageSize
        //        };

        //        return ApiResponse<PagedResult<PendingAppraisalResponseDto>>.Success(response);
        //    });
        //}

        public async Task<ApiResponse<PagedResult<OverdueDocumentDto>>> GetOverdueDocumentsAsync(Guid managerId, OverdueFilterDto filter)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var result = await _unitOfWork.TechnicalDocument.GetOverdueDocumentsAsync(managerId, filter);

                var response = new PagedResult<OverdueDocumentDto>
                {
                    Items = _mapper.Map<List<OverdueDocumentDto>>(result.Items),
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize
                };

                return ApiResponse<PagedResult<OverdueDocumentDto>>.Success(response);
            });
        }

        public async Task<ApiResponse<PagedResult<OverdueDocumentDto>>> GetManagerRequestOverdueDocumentsAsync(Guid directorId, OverdueFilterDto filter)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var result = await _unitOfWork.TechnicalDocument.GetManagerRequestOverdueDocumentsAsync(directorId, filter);

                var response = new PagedResult<OverdueDocumentDto>
                {
                    Items = _mapper.Map<List<OverdueDocumentDto>>(result.Items),
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize
                };

                return ApiResponse<PagedResult<OverdueDocumentDto>>.Success(response);
            });
        }

        public async Task<ApiResponse<PagedResult<ManagerDashboardDocumentDto>>> GetManagerRequestStatusDocumentsAsync(Guid managerId, DepartmentDocumentStatusFilterDto filter)
        {
            var result = await _unitOfWork.TechnicalDocument.GetManagerRequestStatusDocumentsAsync(managerId, filter);
            var mapped = _mapper.Map<PagedResult<ManagerDashboardDocumentDto>>(result);

            return ApiResponse<PagedResult<ManagerDashboardDocumentDto>>.Success(mapped);
        }

        public async Task<ApiResponse<PagedResult<IncomingAppraisalDocumentDto>>> GetIncomingAppraisalDocumentsAsync(Guid coordinatorId, PaginationDto pagination, string? searchTerm)
        {
            var coordinator = await _unitOfWork.Users.GetByIdAsync(coordinatorId);
            if (coordinator?.DepartmentId == null) return ApiResponse<PagedResult<IncomingAppraisalDocumentDto>>.Failure(404, "Không tìm thấy đơn vị.");

            var result = await _unitOfWork.TechnicalDocument.GetIncomingAppraisalDocumentsAsync(coordinator.DepartmentId.Value, pagination, searchTerm);
            var mapped = _mapper.Map<PagedResult<IncomingAppraisalDocumentDto>>(result);

            return ApiResponse<PagedResult<IncomingAppraisalDocumentDto>>.Success(mapped);
        }

        public async Task<ApiResponse<PagedResult<IncomingAppraisalDocumentDto>>> GetUserWorkloadDocumentsAsync(Guid userId, PaginationDto pagination, string? searchTerm)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null) return ApiResponse<PagedResult<IncomingAppraisalDocumentDto>>.Failure(404, "Không tìm thấy người dùng.");

                var result = await _unitOfWork.TechnicalDocument.GetUserWorkloadDocumentsAsync(userId, pagination, searchTerm);

                var mapped = _mapper.Map<PagedResult<IncomingAppraisalDocumentDto>>(result);

                return ApiResponse<PagedResult<IncomingAppraisalDocumentDto>>.Success(mapped);
            }
            );
        }
    }
}