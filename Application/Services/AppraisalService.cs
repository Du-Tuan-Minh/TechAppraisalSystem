using Application.Common;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Application.Services
{
    public class AppraisalService : BaseService, IAppraisalService
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public AppraisalService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService) : base(unitOfWork)
        {
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<bool>> CreateParallelAssignmentsAsync(CreateParallelAssignmentsRequest request, Guid senderId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(request.DocumentId);
                if (doc == null) return ApiResponse<bool>.Failure(404, "Tài liệu không tồn tại.");

                if (doc.Status != DocumentStatus.InternalApproved && doc.Status != DocumentStatus.AdjustmentRequired && doc.Status != DocumentStatus.AppraisalPending && doc.Status != DocumentStatus.Signing)
                {
                    return ApiResponse<bool>.Failure(400, "Trạng thái tài liệu hiện tại không cho phép phân phối thẩm định.");
                }

                var roleSenior = await _unitOfWork.Users.GetUserRoleByIdAsync(senderId);

                User? manager = null;
                bool isSenior = false;
                Guid? departmentSeniorId = null;

                foreach (var assignInfo in request.DepartmentAssignments)
                {
                    if (assignInfo.Deadline.HasValue)
                    {
                        var now = DateTime.UtcNow;

                        if (assignInfo.Deadline.Value < now)
                        {
                            return ApiResponse<bool>.Failure(400, "Deadline của đơn vị phải lớn hơn hoặc bằng thời gian hiện tại.");
                        }

                        if (request.GlobalDeadline.HasValue && assignInfo.Deadline.Value > request.GlobalDeadline.Value)
                        {
                            return ApiResponse<bool>.Failure(400, "Deadline của đơn vị không được lớn hơn deadline tổng.");
                        }
                    }

                    Guid departmentId;
                    bool isExist;

                    if (roleSenior == UserRole.Director)
                    {
                        isSenior = false;

                        manager = await _unitOfWork.Users.GetUnitLeaderAsync(assignInfo.TargetId);
                        if (manager == null) return ApiResponse<bool>.Failure(404, $"Không tìm thấy lãnh đạo cho phòng ban có ID: {assignInfo.TargetId}");

                        departmentId = assignInfo.TargetId;

                        isExist = await _unitOfWork.AppraisalAssignments.AnyAsync(a =>
                            a.RequestVersionId == request.RequestVersionId &&
                            a.DepartmentId == departmentId);
                    }
                    else if (roleSenior == UserRole.InstituteDirector)
                    {
                        manager = await _unitOfWork.Users.GetByIdAsync(assignInfo.TargetId);
                        if (manager == null) return ApiResponse<bool>.Failure(404, $"Không tìm thấy phó viện trưởng có ID: {assignInfo.TargetId}");

                        departmentSeniorId = manager.DepartmentId;
                        departmentId = departmentSeniorId.Value;

                        isSenior = true;

                        isExist = await _unitOfWork.AppraisalAssignments.AnyAsync(a =>
                            a.RequestVersionId == request.RequestVersionId &&
                            a.ResponsibleManagerId == manager.Id);
                    }
                    else
                    {
                        continue;
                    }

                    if (isExist) continue;

                    var assignment = new AppraisalAssignment
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = request.DocumentId,
                        RequestVersionId = request.RequestVersionId,
                        DepartmentId = departmentId,
                        ResponsibleManagerId = manager.Id,
                        AssignedById = senderId,
                        Deadline = assignInfo.Deadline ?? request.GlobalDeadline,
                        ManagerComment = assignInfo.ManagerComment ?? request.GlobalComment,
                        Status = AssignmentStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.AppraisalAssignments.AddAsync(assignment);

                    var notificationTitle = roleSenior == UserRole.Director
                        ? $"Nhiệm vụ thẩm định mới từ giám đốc trung tâm: {manager.Profile.FullName}"
                        : $"Nhiệm vụ thẩm định mới từ viện trưởng: {manager.Profile.FullName}";

                    if (manager.Id != senderId)
                    {
                        await _notificationService.SendNotificationWithRouteAsync(
                              NotificationRouteType.ManagerAssignment,
                              manager.Id,
                              notificationTitle,
                              $"Bạn được giao nhiệm vụ chủ trì thẩm định tài liệu: {doc.Title} (Mã: {doc.DocumentCode})",
                              doc.Id,
                              assignment.Id,
                              request.RequestVersionId,
                              null,
                              departmentId
                          );
                    }
                }

                var isDeputyInstituteDirector = roleSenior == UserRole.DeputyInstituteDirector;

                if (isDeputyInstituteDirector)
                {
                    var instituteDirectorId = await _unitOfWork.Users.GetFirstUserIdByRoleAsync(UserRole.InstituteDirector);
                    var instituteDirector = await _unitOfWork.Users.GetByIdAsync(instituteDirectorId);

                    departmentSeniorId = instituteDirector.DepartmentId;

                    var isExist = await _unitOfWork.AppraisalAssignments.AnyAsync(a =>
                        a.RequestVersionId == request.RequestVersionId &&
                        a.ResponsibleManagerId == instituteDirectorId.Value);

                    if (!isExist)
                    {
                        var assignment = new AppraisalAssignment
                        {
                            Id = Guid.NewGuid(),
                            DocumentId = request.DocumentId,
                            RequestVersionId = request.RequestVersionId,
                            DepartmentId = departmentSeniorId.Value,
                            ResponsibleManagerId = instituteDirectorId.Value,
                            AssignedById = senderId,
                            Deadline = request.GlobalDeadline,
                            ManagerComment = request.GlobalComment,
                            Status = AssignmentStatus.Pending,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.AppraisalAssignments.AddAsync(assignment);

                        await _notificationService.SendNotificationWithRouteAsync(
                              NotificationRouteType.DirectorAssignment,
                              instituteDirectorId.Value,
                              $"Đề xuất thẩm định từ phó viện trưởng: {instituteDirector.Profile.FullName}",
                              $"Bạn được đề xuất chủ trì thẩm định tài liệu: {doc.Title} (Mã: {doc.DocumentCode})",
                              doc.Id,
                              assignment.Id,
                              request.RequestVersionId,
                              null,
                              departmentSeniorId
                          );
                    }
                }

                var oldStatus = doc.Status;
                doc.CurrentHandlerId = null;
                doc.Status = isDeputyInstituteDirector ? DocumentStatus.Signing : DocumentStatus.AppraisalPending;
                _unitOfWork.TechnicalDocument.Update(doc);

                var history = new AppraisalHistory
                {
                    Id = Guid.NewGuid(),
                    DocumentId = request.DocumentId,
                    RequestVersionId = request.RequestVersionId,
                    HandlerId = senderId,
                    OldStatus = oldStatus,
                    NewStatus = doc.Status,
                    Comment = request.GlobalComment ?? "Phân phối thẩm định song song tới các đơn vị.",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AppraisalHistory.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Tài liệu đã được phân phối thẩm định song song tới các đơn vị thành công.");
            });
        }

        public async Task<ApiResponse<bool>> AssignInternalStaffAsync(AssignStaffRequest request, Guid managerId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var assignment = await _unitOfWork.AppraisalAssignments.GetByIdAsync(
                    request.AssignmentId,
                    a => a.Reviewers,
                    a => a.Document);

                if (assignment == null) return ApiResponse<bool>.Failure(404, "Nhiệm vụ thẩm định không tồn tại.");
                if (assignment.ResponsibleManagerId != managerId) return ApiResponse<bool>.Failure(403, "Bạn không có quyền thực hiện thao tác này.");

                var now = DateTime.UtcNow;

                if (assignment.Deadline.HasValue)
                {
                    if (assignment.Deadline.Value < now)
                    {
                        return ApiResponse<bool>.Failure(400, "Deadline của nhiệm vụ thẩm định đã quá hạn.");
                    }

                    if (request.Deadline.HasValue)
                    {
                        if (request.Deadline.Value < now)
                        {
                            return ApiResponse<bool>.Failure(400, "Deadline của nhân viên phải lớn hơn hoặc bằng thời điểm hiện tại.");
                        }

                        if (request.Deadline.Value > assignment.Deadline.Value)
                        {
                            return ApiResponse<bool>.Failure(400, "Deadline của nhân viên không được vượt quá deadline của nhiệm vụ.");
                        }
                    }
                }

                var existingStaffIds = assignment.Reviewers.Select(r => r.StaffId).ToHashSet();
                var newReviewers = new List<AppraisalReviewer>();

                foreach (var staffId in request.StaffIds)
                {
                    if (existingStaffIds.Contains(staffId)) continue;

                    newReviewers.Add(new AppraisalReviewer
                    {
                        Id = Guid.NewGuid(),
                        AssignmentId = request.AssignmentId,
                        StaffId = staffId,
                        Status = ReviewerStatus.Reviewing,
                        TaskDescription = request.ManagerNote,
                        CreatedAt = DateTime.UtcNow,
                        Deadline = request.Deadline ?? assignment.Deadline,
                        Comment = request.ManagerNote
                    });
                }

                if (!newReviewers.Any()) return ApiResponse<bool>.Success(true, "Không có nhân viên mới nào được gán.");

                await _unitOfWork.AppraisalReviewers.AddRangeAsync(newReviewers);

                if (assignment.Document.Status == DocumentStatus.AppraisalPending)
                {
                    assignment.Document.Status = DocumentStatus.Appraising;
                    _unitOfWork.TechnicalDocument.Update(assignment.Document);
                }

                if (assignment.Status == AssignmentStatus.Pending)
                {
                    assignment.Status = AssignmentStatus.InReview;
                    _unitOfWork.AppraisalAssignments.Update(assignment);
                }

                var history = new AppraisalHistory
                {
                    Id = Guid.NewGuid(),
                    DocumentId = assignment.DocumentId,
                    RequestVersionId = assignment.RequestVersionId,
                    HandlerId = managerId,
                    OldStatus = DocumentStatus.AppraisalPending,
                    NewStatus = DocumentStatus.Appraising,
                    Comment = $"Phân công {newReviewers.Count} chuyên viên thẩm định.",
                    AppraisalAssignmentId = assignment.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AppraisalHistory.AddAsync(history);

                var result = await _unitOfWork.SaveChangesAsync() > 0;

                if (result)
                {
                    foreach (var r in newReviewers)
                    {
                        await _notificationService.SendNotificationWithRouteAsync(
                              NotificationRouteType.StaffReview,
                              r.StaffId,
                              "Nhiệm vụ thẩm định mới",
                              $"Bạn được phân công thẩm định tài liệu: {assignment.Document.Title}",
                              assignment.DocumentId,
                              assignment.Id,
                              assignment.RequestVersionId,
                              r.Id,
                              assignment.DepartmentId
                          );
                    }
                }

                return ApiResponse<bool>.Success(result, $"Đã phân công thành công cho {newReviewers.Count} nhân viên.");
            });
        }

        public async Task<ApiResponse<bool>> SubmitStaffReviewAsync(Guid reviewerId, UpdateReviewerProgressRequest dto, Guid staffId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var reviewer = await _unitOfWork.AppraisalReviewers.GetByIdAsync(
                    reviewerId,
                    r => r.Assignment,
                    r => r.Staff,
                    r => r.Staff.Profile,
                    r => r.Assignment.Document);

                if (reviewer == null) return ApiResponse<bool>.Failure(404, "Nhiệm vụ chuyên trách không tồn tại.");
                if (reviewer.StaffId != staffId) return ApiResponse<bool>.Failure(403, "Bạn không có quyền thực hiện nhiệm vụ này.");

                reviewer.Status = dto.Status;
                reviewer.Comment = dto.Comment;

                if (dto.Status == ReviewerStatus.Completed)
                {
                    reviewer.CompletedAt = DateTime.UtcNow;
                }

                if (dto.NewIssues != null && dto.NewIssues.Any())
                {
                    var issues = dto.NewIssues.Select(issueDto => new FeedbackIssue
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = reviewer.Assignment.DocumentId,
                        RequestVersionId = reviewer.Assignment.RequestVersionId,
                        ReporterId = staffId,
                        IndicatorPath = issueDto.IndicatorPath,
                        Description = issueDto.Description,
                        IssueCategory = issueDto.IssueCategory,
                        Severity = issueDto.Severity,
                        Status = IssueStatus.New,
                        AppraisalHistoryId = issueDto.AppraisalHistoryId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _unitOfWork.FeedbackIssues.AddRangeAsync(issues);
                }

                //if (dto.AttachmentIds != null && dto.AttachmentIds.Any())
                //{
                //    foreach (var attachmentId in dto.AttachmentIds)
                //    {
                //        var link = new AttachmentLink
                //        {
                //            Id = Guid.NewGuid(),
                //            AttachmentId = attachmentId,
                //            EntityId = reviewer.Id,
                //            EntityType = LinkedEntityType.AppraisalReviewer
                //            CreatedAt = DateTime.UtcNow
                //            
                //        };
                //        await _unitOfWork.AttachmentLinks.AddAsync(link);
                //    }
                //}

                if (dto.Status == ReviewerStatus.Completed)
                {
                    await _notificationService.SendSystemNotificationAsync(
                        reviewer.Assignment.ResponsibleManagerId,
                        "Nhân viên đã hoàn thành thẩm định",
                        $"Nhân viên chuyên trách '{reviewer.Staff.Profile.FullName}' đã hoàn thành thẩm định tài liệu '{reviewer.Assignment.Document.Title}'.",
                         JsonSerializer.Serialize(new
                         {
                             documentId = reviewer.Assignment.DocumentId,
                             versionId = reviewer.Assignment.RequestVersionId
                         }));
                }

                _unitOfWork.AppraisalReviewers.Update(reviewer);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Gửi kết quả thẩm định chuyên trách thành công.");
            });
        }

        public async Task<ApiResponse<bool>> ConfirmDepartmentResultAsync(Guid documentId, CompleteAssignmentRequest request, Guid managerId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(managerId);
                if (manager == null) return ApiResponse<bool>.Failure(404, "Người xác nhận không tồn tại.");
                var managerRole = await _unitOfWork.Users.GetUserRoleByIdAsync(managerId);

                var document = await _unitOfWork.TechnicalDocument.GetByIdAsync(documentId);
                if (document == null) return ApiResponse<bool>.Failure(404, "Tài liệu kỹ thuật không tồn tại.");

                var oldDocumentStatus = document.Status;

                var assignment = await _unitOfWork.AppraisalAssignments.GetByIdAsync(
                    request.AssignmentId,
                    a => a.Reviewers);

                if (assignment == null || assignment.DocumentId != documentId || assignment.ResponsibleManagerId != managerId)
                    return ApiResponse<bool>.Failure(403, "Nhiệm vụ không hợp lệ hoặc bạn không có quyền xác nhận.");

                if (assignment.Reviewers.Any(r => r.Status == ReviewerStatus.Reviewing))
                    return ApiResponse<bool>.Failure(400, "Vẫn còn nhân viên chuyên trách chưa nộp kết quả.");

                if (request.ValidatedIssueIds.Any())
                {
                    var validIssues = await _unitOfWork.FeedbackIssues.FindAsync(i => request.ValidatedIssueIds.Contains(i.Id));
                    foreach (var i in validIssues) i.Status = IssueStatus.InProcessing;
                    _unitOfWork.FeedbackIssues.UpdateRange(validIssues);
                }

                if (request.RejectedIssueIds.Any())
                {
                    var invalidIssues = await _unitOfWork.FeedbackIssues.FindAsync(i => request.RejectedIssueIds.Contains(i.Id));
                    foreach (var i in invalidIssues) i.Status = IssueStatus.Rejected;
                    _unitOfWork.FeedbackIssues.UpdateRange(invalidIssues);
                }

                assignment.Status = AssignmentStatus.Completed;
                assignment.ManagerComment = request.FinalComment;
                assignment.CompletedAt = DateTime.UtcNow;
                _unitOfWork.AppraisalAssignments.Update(assignment);

                var hasAnyOtherAssignment = await _unitOfWork.AppraisalAssignments.AnyAsync(a =>
                    a.DocumentId == documentId &&
                    a.Id != assignment.Id &&
                    a.Status != AssignmentStatus.Completed &&
                    a.DepartmentId == assignment.DepartmentId);

                var hasAnyIssue = request.ValidatedIssueIds.Any() || request.RejectedIssueIds.Any();

                if (managerRole == UserRole.DeputyInstituteDirector && !hasAnyOtherAssignment && !hasAnyIssue)
                {
                    if (string.IsNullOrEmpty(document.DocumentCode))
                    {
                        document.DocumentCode = $"VTS-{DateTime.UtcNow.Year}-{document.Id.ToString().Substring(0, 8).ToUpper()}";
                    }

                    document.QrCode = $"VTS-SIGN-{document.DocumentCode}-{DateTime.UtcNow.Ticks}";
                    document.Status = DocumentStatus.Issued;
                    document.CurrentHandlerId = null;
                }

                if (!(managerRole == UserRole.DeputyInstituteDirector && !hasAnyOtherAssignment && !hasAnyIssue))
                {
                    var otherUnitsActive = await _unitOfWork.AppraisalAssignments.AnyAsync(a =>
                        a.DocumentId == documentId &&
                        a.Id != assignment.Id &&
                        a.Status != AssignmentStatus.Completed &&
                        a.DepartmentId == assignment.DepartmentId &&
                        (a.ResponsibleManager.Role == UserRole.DeputyInstituteDirector || a.ResponsibleManager.Role == UserRole.Manager));

                    if (!otherUnitsActive)
                    {
                        document.Status = DocumentStatus.Signing;
                        var director = await _unitOfWork.Users.GetDirectorByDepartmentAsync(manager.DepartmentId!.Value);
                        document.CurrentHandlerId = director?.Id;
                    }
                }

                //if (request.AttachmentIds?.Any() == true)
                //{
                //    var newLinks = request.AttachmentIds.Select(attId => new AttachmentLink
                //    {
                //        Id = Guid.NewGuid(),
                //        AttachmentId = attId,
                //        EntityId = assignment.Id,
                //        EntityType = LinkedEntityType.AppraisalAssignment,
                //        CreatedAt = DateTime.UtcNow
                //    });
                //    await _unitOfWork.AttachmentLinks.AddRangeAsync(newLinks);
                //}

                var history = new AppraisalHistory
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    HandlerId = managerId,
                    RequestVersionId = assignment.RequestVersionId,
                    OldStatus = oldDocumentStatus,
                    NewStatus = document.Status,
                    AppraisalAssignmentId = assignment.Id,
                    Comment = request.FinalComment ?? $"Đơn vị {assignment.DepartmentId} đã hoàn thành thẩm định.",
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.TechnicalDocument.Update(document);
                await _unitOfWork.AppraisalHistory.AddAsync(history);

                await _unitOfWork.SaveChangesAsync();

                var isSenior = managerRole == UserRole.DeputyInstituteDirector;

                var confirmMessage = isSenior
                    ? $"Tài liệu '{document.Title}' đã được thông qua kết quả thẩm định bởi cấp viện do {manager.Profile.FullName} xác nhận."
                    : $"Tài liệu '{document.Title}' đã được thông qua kết quả thẩm định bởi cấp trung tâm do {manager.Profile.FullName} xác nhận.";

                var notifiedUserIds = new HashSet<Guid>();

                foreach (var reviewer in assignment.Reviewers)
                {
                    if (reviewer.StaffId != Guid.Empty &&
                        notifiedUserIds.Add(reviewer.StaffId))
                    {
                        await _notificationService.SendNotificationWithRouteAsync(
                              NotificationRouteType.AssignmentDetail,
                              reviewer.StaffId,
                              "Kết quả thẩm định đã được xác nhận",
                              confirmMessage,
                              document.Id,
                              assignment.Id,
                              assignment.RequestVersionId,
                              null,
                              assignment.DepartmentId
                          );
                    }
                }

                return ApiResponse<bool>.Success(true, "Xác nhận kết quả thẩm định đơn vị thành công.");
            });
        }

        public async Task<ApiResponse<bool>> ConfirmCenterResultAsync(ConsolidateAppraisalRequest request, Guid directorId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(request.DocumentId, d => d.Versions, d => d.Department);
                if (doc == null) return ApiResponse<bool>.Failure(404, "The document does not exist.");

                var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);
                if (currentVersion == null) return ApiResponse<bool>.Failure(400, "The document does not have a current version.");

                var pendingDepts = await _unitOfWork.AppraisalAssignments
                    .GetAllAsQueryable()
                    .AnyAsync(a =>
                       a.DocumentId == request.DocumentId &&
                       a.Department != null &&
                       a.Department.ParentId != null &&
                       (a.Status == AssignmentStatus.Pending || a.Status == AssignmentStatus.InReview)
                );

                if (pendingDepts) return ApiResponse<bool>.Failure(400, "Some units have yet to complete the assessment.");

                var oldStatus = doc.Status;

                if (request.ValidatedIssueIds.Any())
                {
                    var validIssues = await _unitOfWork.FeedbackIssues.FindAsync(i => request.ValidatedIssueIds.Contains(i.Id));
                    foreach (var i in validIssues)
                    {
                        i.Status = IssueStatus.InProcessing;
                        _unitOfWork.FeedbackIssues.Update(i);
                    }
                }

                if (request.RejectedIssueIds.Any())
                {
                    var invalidIssues = await _unitOfWork.FeedbackIssues.FindAsync(i => request.RejectedIssueIds.Contains(i.Id));
                    foreach (var i in invalidIssues)
                    {
                        i.Status = IssueStatus.Rejected;
                        _unitOfWork.FeedbackIssues.Update(i);
                    }
                }

                if (!request.IsPass)
                {
                    doc.Status = DocumentStatus.AdjustmentRequired;
                    doc.CurrentHandlerId = doc.RequesterId;

                    await _notificationService.SendNotificationWithRouteAsync(
                        NotificationRouteType.RejectedDocument,
                        doc.RequesterId,
                        "Hồ sơ bị từ chối thẩm định",
                        $"Tài liệu {doc.Title} đã bị trả về để chỉnh sửa sau vòng thẩm định đa bên.",
                        doc.Id,
                        versionId: currentVersion.Id
                    );
                }
                else
                {
                    var approvalProposerId = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(doc.ApprovalProposerIds) ?? new List<Guid>();
                    var instituteDirectorId = await _unitOfWork.Users.GetFirstUserIdByRoleAsync(UserRole.InstituteDirector);
                    var senior = await _unitOfWork.Users.GetByIdAsync(instituteDirectorId);

                    if (approvalProposerId.Count > 0)
                    {
                        doc.Status = DocumentStatus.Signing;
                        doc.CurrentHandlerId = approvalProposerId.First();

                        var isExist = await _unitOfWork.AppraisalAssignments.AnyAsync(a =>
                            a.RequestVersionId == currentVersion.Id &&
                            a.ResponsibleManagerId == approvalProposerId.First());

                        if (!isExist)
                        {
                            var assignment = new AppraisalAssignment
                            {
                                Id = Guid.NewGuid(),
                                DocumentId = doc.Id,
                                DepartmentId = senior.DepartmentId.Value,
                                ResponsibleManagerId = approvalProposerId.First(),
                                AssignedById = directorId,
                                RequestVersionId = currentVersion.Id,
                                Status = AssignmentStatus.Pending
                            };

                            await _unitOfWork.AppraisalAssignments.AddAsync(assignment);

                            await _notificationService.SendNotificationWithRouteAsync(
                                NotificationRouteType.FinalApproval,
                                approvalProposerId.First(),
                                $"Hồ sơ chờ phê duyệt cuối cùng được tạo bởi trung tâm: {doc.Department.NameDepartment}",
                                $"Tài liệu {doc.Title} đã qua vòng thẩm định đa bên, chờ bạn phê duyệt.",
                                doc.Id,
                                assignmentId: assignment.Id,
                                versionId: currentVersion.Id
                            );
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(doc.DocumentCode))
                        {
                            doc.DocumentCode = $"VTS-{DateTime.UtcNow.Year}-{doc.Id.ToString().Substring(0, 8).ToUpper()}";
                        }

                        doc.QrCode = $"VTS-SIGN-{doc.DocumentCode}-{DateTime.UtcNow.Ticks}";
                        doc.Status = DocumentStatus.Issued;
                        doc.CurrentHandlerId = null;
                    }
                }

                var history = new AppraisalHistory
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    RequestVersionId = request.RequestVersionId,
                    HandlerId = directorId,
                    OldStatus = oldStatus,
                    NewStatus = doc.Status,
                    Comment = request.FinalComment ?? (request.IsPass ? "Hồ sơ đạt yêu cầu." : "Hồ sơ cần sửa đổi theo danh sách lỗi tổng hợp."),
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.TechnicalDocument.Update(doc);
                await _unitOfWork.AppraisalHistory.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                //if (request.AttachmentIds?.Any() == true)
                //{
                //    var newLinks = request.AttachmentIds.Select(attId => new AttachmentLink
                //    {
                //        Id = Guid.NewGuid(),
                //        AttachmentId = attId,
                //        EntityId = historyId, // Link vào History tổng
                //        EntityType = LinkedEntityType.AppraisalHistory,
                //        CreatedAt = DateTime.UtcNow
                //    });
                //    await _unitOfWork.AttachmentLinks.AddRangeAsync(newLinks);
                //}

                var notifyMessage = request.IsPass
                    ? $"Tài liệu '{doc.Title}' đã hoàn tất thẩm định và được phát hành chính thức."
                    : $"Tài liệu '{doc.Title}' cần chỉnh sửa bổ sung sau quá trình tổng hợp thẩm định.";

                var notifiedUserIds = new HashSet<Guid>();

                // Người tạo tài liệu
                if (doc.RequesterId != Guid.Empty &&
                    notifiedUserIds.Add(doc.RequesterId))
                {
                    await _notificationService.SendNotificationWithRouteAsync(
                        request.IsPass
                            ? NotificationRouteType.IssuedDocument
                            : NotificationRouteType.RejectedDocument,
                        doc.RequesterId,
                        "Cập nhật kết quả tài liệu",
                        notifyMessage,
                        doc.Id,
                        versionId: currentVersion.Id
                    );
                }

                // Các manager thẩm định
                var assignmentUsers = await _unitOfWork.AppraisalAssignments.FindAsync(a =>
                    a.DocumentId == doc.Id);

                foreach (var assignment in assignmentUsers)
                {
                    if (assignment.ResponsibleManagerId != Guid.Empty &&
                        notifiedUserIds.Add(assignment.ResponsibleManagerId))
                    {
                        await _notificationService.SendNotificationWithRouteAsync(
                            NotificationRouteType.AssignmentDetail,
                            assignment.ResponsibleManagerId,
                            "Cập nhật kết quả tài liệu",
                            notifyMessage,
                            doc.Id,
                            assignmentId: assignment.Id,
                            versionId: currentVersion.Id,
                            departmentId: assignment.DepartmentId
                        );
                    }
                }

                // Các reviewer chuyên trách
                var assignmentIds = assignmentUsers.Select(a => a.Id).ToList();

                var reviewers = await _unitOfWork.AppraisalReviewers.FindAsync(r =>
                    assignmentIds.Contains(r.AssignmentId));

                foreach (var reviewer in reviewers)
                {
                    if (reviewer.StaffId != Guid.Empty &&
                        notifiedUserIds.Add(reviewer.StaffId))
                    {
                        await _notificationService.SendNotificationWithRouteAsync(
                            NotificationRouteType.StaffReview,
                            reviewer.StaffId,
                            "Cập nhật kết quả tài liệu",
                            notifyMessage,
                            doc.Id,
                            versionId: currentVersion.Id,
                            reviewerId: reviewer.Id
                        );
                    }
                }

                return ApiResponse<bool>.Success(true, "Đã hoàn tất tổng hợp và chốt kết quả thẩm định.");
            });
        }

        public async Task<ApiResponse<PagedResult<AppraisalAssignmentDto>>> GetAssignmentsForDirectorAsync(Guid directorId, PaginationDto pagination)
        {
            var director = await _unitOfWork.Users.GetByIdAsync(directorId);
            if (director == null || (director.Role != UserRole.Director && director.Role != UserRole.InstituteDirector && director.Role != UserRole.DeputyInstituteDirector)) return ApiResponse<PagedResult<AppraisalAssignmentDto>>.Failure(403, "Bạn không có quyền truy cập.");

            var pagedEntities = await _unitOfWork.AppraisalAssignments.GetDirectorAssignmentsAsync(
                director.Id,
                null,
                pagination.Page,
                pagination.PageSize);

            var dtos = _mapper.Map<List<AppraisalAssignmentDto>>(pagedEntities.Items);

            var result = new PagedResult<AppraisalAssignmentDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            };

            return ApiResponse<PagedResult<AppraisalAssignmentDto>>.Success(result);
        }

        public async Task<ApiResponse<PagedResult<AppraisalAssignmentDto>>> GetAssignmentsForManagerAsync(Guid managerId, Guid? versionId, PaginationDto pagination)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(managerId);
            if (user == null || (user.Role != UserRole.Director && user.Role != UserRole.Manager && user.Role != UserRole.InstituteDirector && user.Role != UserRole.DeputyInstituteDirector) || !user.DepartmentId.HasValue)
                return ApiResponse<PagedResult<AppraisalAssignmentDto>>.Failure(403, "Bạn không có quyền truy cập.");

            var pagedEntities = await _unitOfWork.AppraisalAssignments.GetManagerAssignmentsAsync(
                 managerId, user.DepartmentId.Value, versionId, user.Role, pagination.Page, pagination.PageSize);

            var dtos = _mapper.Map<List<AppraisalAssignmentDto>>(pagedEntities.Items);

            return ApiResponse<PagedResult<AppraisalAssignmentDto>>.Success(new PagedResult<AppraisalAssignmentDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            });
        }

        public async Task<ApiResponse<AppraisalAssignmentDetailDto>> GetAssignmentDetailAsync(Guid assignmentId)
        {
            var assignment = await _unitOfWork.AppraisalAssignments.GetWithDetailsAsync(assignmentId);

            if (assignment == null) return ApiResponse<AppraisalAssignmentDetailDto>.Failure(404, "No records found.");

            return ApiResponse<AppraisalAssignmentDetailDto>.Success(_mapper.Map<AppraisalAssignmentDetailDto>(assignment));
        }

        public async Task<ApiResponse<AppraisalReviewerDetailDto>> GetReviewerDetailAsync(Guid reviewerId)
        {
            var reviewer = await _unitOfWork.AppraisalReviewers.GetWithDetailsAsync(reviewerId);

            if (reviewer == null) return ApiResponse<AppraisalReviewerDetailDto>.Failure(404, "Không tìm thấy thông tin chuyên viên.");

            return ApiResponse<AppraisalReviewerDetailDto>.Success(_mapper.Map<AppraisalReviewerDetailDto>(reviewer));
        }

        public async Task<ApiResponse<PagedResult<AppraisalReviewerDto>>> GetAssignmentsReviewForStaffAsync(Guid? assignmentId, Guid userId, PaginationDto pagination)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            var pagedEntities = await _unitOfWork.AppraisalReviewers.GetByReviewerIdAsync(assignmentId, userId, user.Role, pagination);

            var dtos = _mapper.Map<List<AppraisalReviewerDto>>(pagedEntities.Items);

            return ApiResponse<PagedResult<AppraisalReviewerDto>>.Success(new PagedResult<AppraisalReviewerDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            });
        }

        public async Task<ApiResponse<bool>> RecallAssignmentsAsync(Guid documentId, Guid directorId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(documentId, d => d.Versions);
                if (doc == null) return ApiResponse<bool>.Failure(404, "Tài liệu không tồn tại.");


                var director = await _unitOfWork.Users.GetByIdAsync(directorId);
                if (director == null || director.Role != UserRole.Director) return ApiResponse<bool>.Failure(403, "Bạn không có quyền.");

                var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);
                if (currentVersion == null) return ApiResponse<bool>.Failure(400, "Tài liệu không có phiên bản hiện hành.");

                if (doc.Status != DocumentStatus.Appraising && doc.Status != DocumentStatus.AppraisalPending)
                {
                    return ApiResponse<bool>.Failure(400, "Trạng thái tài liệu không cho phép thu hồi.");
                }

                var assignments = await _unitOfWork.AppraisalAssignments.FindAsync(a =>
                    a.DocumentId == documentId &&
                    a.RequestVersionId == currentVersion.Id &&
                    (a.Status == AssignmentStatus.Pending || a.Status == AssignmentStatus.InReview));

                if (!assignments.Any()) return ApiResponse<bool>.Success(true, "Không có assignment cần thu hồi.");

                var assignmentIds = assignments.Select(a => a.Id).ToList();

                var reviewers = await _unitOfWork.AppraisalReviewers.FindAsync(r =>
                    assignmentIds.Contains(r.AssignmentId) &&
                    (r.Status == ReviewerStatus.Pending || r.Status == ReviewerStatus.Reviewing));

                foreach (var r in reviewers)
                {
                    r.Status = ReviewerStatus.Skipped;
                    r.CompletedAt = DateTime.UtcNow;
                }

                _unitOfWork.AppraisalReviewers.UpdateRange(reviewers);

                foreach (var a in assignments)
                {
                    a.Status = AssignmentStatus.Skipped;
                    a.CompletedAt = DateTime.UtcNow;
                }

                _unitOfWork.AppraisalAssignments.UpdateRange(assignments);

                var oldStatus = doc.Status;
                doc.Status = DocumentStatus.Signing;
                doc.CurrentHandlerId = directorId;

                _unitOfWork.TechnicalDocument.Update(doc);

                var history = new AppraisalHistory
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    RequestVersionId = currentVersion.Id,
                    HandlerId = directorId,
                    OldStatus = oldStatus,
                    NewStatus = doc.Status,
                    Comment = "Giám đốc trung tâm đã thu hồi toàn bộ nhiệm vụ thẩm định chưa hoàn thành.",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.AppraisalHistory.AddAsync(history);

                var notifiedUserIds = new HashSet<Guid>();

                foreach (var assignment in assignments)
                {
                    if (assignment.ResponsibleManagerId != Guid.Empty &&
                        notifiedUserIds.Add(assignment.ResponsibleManagerId))
                    {
                        await _notificationService.SendNotificationWithRouteAsync(
                            NotificationRouteType.AssignmentDetail,
                            assignment.ResponsibleManagerId,
                            "Nhiệm vụ thẩm định đã bị thu hồi",
                            $"Nhiệm vụ thẩm định tài liệu '{doc.Title}' đã bị thu hồi bởi Giám đốc trung tâm '{director.Profile.FullName}'.",
                            doc.Id,
                            assignmentId: assignment.Id,
                            versionId: currentVersion.Id,
                            departmentId: assignment.DepartmentId
                        );
                    }
                }

                foreach (var reviewer in reviewers)
                {
                    if (reviewer.StaffId != Guid.Empty &&
                        notifiedUserIds.Add(reviewer.StaffId))
                    {
                        await _notificationService.SendNotificationWithRouteAsync(
                            NotificationRouteType.StaffReview,
                            reviewer.StaffId,
                            "Nhiệm vụ thẩm định đã bị thu hồi",
                            $"Nhiệm vụ thẩm định tài liệu '{doc.Title}' đã bị thu hồi bởi Giám đốc trung tâm '{director.Profile.FullName}'.",
                            doc.Id,
                            assignmentId: reviewer.AssignmentId,
                            versionId: currentVersion.Id,
                            reviewerId: reviewer.Id
                        );
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Đã thu hồi assignment của version hiện tại.");
            });
        }

        //public async Task<ApiResponse<bool>> CoordinatorAssignAsync(CoordinatorAssignRequest request, Guid coordinatorId)
        //{
        //    return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
        //    {
        //        var document = await _unitOfWork.TechnicalDocument.GetByIdAsync(
        //            request.DocumentId,
        //            d => d.Versions);

        //        if (document == null) return ApiResponse<bool>.Failure(404, "Không tìm thấy tài liệu.");
        //        var currentVersion = document.Versions.FirstOrDefault(x => x.IsCurrent);
        //        if (currentVersion == null) return ApiResponse<bool>.Failure(400, "Không tìm thấy phiên bản hiện tại.");

        //        var assignmentMap = new Dictionary<Guid, AppraisalAssignment>();

        //        // ===== MANAGER ĐƯỢC CHỌN TRỰC TIẾP =====
        //        if (request.ManagerIds?.Any() == true)
        //        {
        //            var managers = await _unitOfWork.Users.FindAsync(
        //                x => request.ManagerIds.Contains(x.Id)
        //                     && x.Role == UserRole.Manager);

        //            foreach (var manager in managers)
        //            {
        //                if (!assignmentMap.ContainsKey(manager.Id))
        //                {
        //                    assignmentMap[manager.Id] = new AppraisalAssignment
        //                    {
        //                        DocumentId = document.Id,
        //                        RequestVersionId = currentVersion.Id,
        //                        AssignedById = coordinatorId,
        //                        ResponsibleManagerId = manager.Id,
        //                        DepartmentId = manager.DepartmentId!.Value,
        //                        Deadline = request.Deadline,
        //                        Status = AssignmentStatus.Pending,
        //                        ManagerComment = request.Comment
        //                    };
        //                }
        //            }
        //        }

        //        // ===== STAFF =====
        //        var reviewers = new List<AppraisalReviewer>();

        //        if (request.StaffIds?.Any() == true)
        //        {
        //            var staffs = await _unitOfWork.Users.FindAsync(
        //                x => request.StaffIds.Contains(x.Id)
        //                     && x.Role == UserRole.Staff);

        //            foreach (var staff in staffs)
        //            {
        //                if (!staff.DepartmentId.HasValue)
        //                    continue;

        //                var manager = await _unitOfWork.Users.GetUnitLeaderAsync(staff.DepartmentId.Value);

        //                if (manager == null)
        //                    continue;

        //                if (!assignmentMap.ContainsKey(manager.Id))
        //                {
        //                    assignmentMap[manager.Id] = new AppraisalAssignment
        //                    {
        //                        DocumentId = document.Id,
        //                        RequestVersionId = currentVersion.Id,
        //                        AssignedById = coordinatorId,
        //                        ResponsibleManagerId = manager.Id,
        //                        DepartmentId = manager.DepartmentId!.Value,
        //                        Deadline = request.Deadline,
        //                        Status = AssignmentStatus.Pending,
        //                        ManagerComment = request.Comment
        //                    };
        //                }

        //                reviewers.Add(new AppraisalReviewer
        //                {
        //                    Assignment = assignmentMap[manager.Id],
        //                    StaffId = staff.Id,
        //                    Deadline = request.Deadline,
        //                    Status = ReviewerStatus.Pending
        //                });
        //            }
        //        }

        //        var assignments = assignmentMap.Values.ToList();

        //        if (assignments.Any()) await _unitOfWork.AppraisalAssignments.AddRangeAsync(assignments);

        //        if (reviewers.Any()) await _unitOfWork.AppraisalReviewers.AddRangeAsync(reviewers);

        //        await _unitOfWork.SaveChangesAsync();

        //        return ApiResponse<bool>.Success(true, "Phân công thành công.");
        //    });
        //}

        public async Task<ApiResponse<bool>> CoordinatorAssignAsync(
    CoordinatorAssignRequest request,
    Guid coordinatorId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var document = await _unitOfWork.TechnicalDocument.GetByIdAsync(request.DocumentId, d => d.Versions);
                if (document == null) return ApiResponse<bool>.Failure(404, "Không tìm thấy tài liệu.");

                var currentVersion = document.Versions.FirstOrDefault(v => v.IsCurrent);
                if (currentVersion.Id != request.VersionId) return ApiResponse<bool>.Failure(400, "Phiên bản không hợp lệ.");

                var existingAssignments = await _unitOfWork.AppraisalAssignments.FindAsync(
                    x => x.RequestVersionId == request.VersionId);

                var assignedManagerIds = existingAssignments
                    .Select(x => x.ResponsibleManagerId)
                    .ToHashSet();

                var existingReviewers = await _unitOfWork.AppraisalReviewers.FindAsync(
                    x => x.Assignment.RequestVersionId == request.VersionId);

                var assignedStaffIds = existingReviewers
                    .Select(x => x.StaffId)
                    .ToHashSet();

                var assignmentMap = new Dictionary<Guid, AppraisalAssignment>();
                var reviewers = new List<AppraisalReviewer>();

                // ===== MANAGER =====
                if (request.ManagerIds?.Any() == true)
                {
                    var managerIds = request.ManagerIds
                        .Distinct()
                        .ToList();

                    var managers = await _unitOfWork.Users.FindAsync(
                        x => managerIds.Contains(x.Id)
                             && x.Role == UserRole.Manager);

                    foreach (var manager in managers)
                    {
                        if (assignedManagerIds.Contains(manager.Id))
                            continue;

                        if (!manager.DepartmentId.HasValue)
                            continue;

                        assignmentMap[manager.Id] = new AppraisalAssignment
                        {
                            DocumentId = document.Id,
                            RequestVersionId = request.VersionId,
                            AssignedById = coordinatorId,
                            ResponsibleManagerId = manager.Id,
                            DepartmentId = manager.DepartmentId.Value,
                            Deadline = request.Deadline,
                            Status = AssignmentStatus.Pending,
                            ManagerComment = request.Comment
                        };
                    }
                }

                // ===== STAFF =====
                if (request.StaffIds?.Any() == true)
                {
                    var staffIds = request.StaffIds
                        .Distinct()
                        .ToList();

                    var staffs = await _unitOfWork.Users.FindAsync(
                        x => staffIds.Contains(x.Id)
                             && x.Role == UserRole.Staff);

                    var departmentIds = staffs
                        .Where(x => x.DepartmentId.HasValue)
                        .Select(x => x.DepartmentId!.Value)
                        .Distinct()
                        .ToList();

                    var departmentManagers = new Dictionary<Guid, User>();

                    foreach (var departmentId in departmentIds)
                    {
                        var manager = await _unitOfWork.Users
                            .GetUnitLeaderAsync(departmentId);

                        if (manager != null)
                        {
                            departmentManagers[departmentId] = manager;
                        }
                    }

                    foreach (var staff in staffs)
                    {
                        if (assignedStaffIds.Contains(staff.Id))
                            continue;

                        if (!staff.DepartmentId.HasValue)
                            continue;

                        if (!departmentManagers.TryGetValue(
                                staff.DepartmentId.Value,
                                out var manager))
                            continue;

                        var existingAssignment = existingAssignments
                            .FirstOrDefault(x => x.ResponsibleManagerId == manager.Id);

                        if (existingAssignment != null)
                        {
                            reviewers.Add(new AppraisalReviewer
                            {
                                AssignmentId = existingAssignment.Id,
                                StaffId = staff.Id,
                                Deadline = request.Deadline,
                                Status = ReviewerStatus.Pending
                            });

                            continue;
                        }

                        if (!assignmentMap.ContainsKey(manager.Id))
                        {
                            assignmentMap[manager.Id] = new AppraisalAssignment
                            {
                                DocumentId = document.Id,
                                RequestVersionId = request.VersionId,
                                AssignedById = coordinatorId,
                                ResponsibleManagerId = manager.Id,
                                DepartmentId = manager.DepartmentId!.Value,
                                Deadline = request.Deadline,
                                Status = AssignmentStatus.Pending,
                                ManagerComment = request.Comment
                            };
                        }

                        reviewers.Add(new AppraisalReviewer
                        {
                            Assignment = assignmentMap[manager.Id],
                            StaffId = staff.Id,
                            Deadline = request.Deadline,
                            Status = ReviewerStatus.Pending
                        });
                    }
                }

                var assignments = assignmentMap.Values.ToList();

                if (assignments.Any())
                {
                    await _unitOfWork.AppraisalAssignments.AddRangeAsync(assignments);
                }

                if (reviewers.Any())
                {
                    await _unitOfWork.AppraisalReviewers.AddRangeAsync(reviewers);
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Phân công thành công.");
            });
        }
    }
}