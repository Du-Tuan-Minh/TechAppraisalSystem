using Application.Common;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class SigningService : BaseService, ISigningService
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public SigningService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService) : base(unitOfWork)
        {
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<bool>> SignDocumentAsync(SendParallelAssignmentsRequest request, Guid userId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
                if (currentUser == null) return ApiResponse<bool>.Failure(404, "Tài khoản không tồn tại.");
                if (currentUser.Role != UserRole.Director) return ApiResponse<bool>.Failure(403, "Chỉ Giám đốc trung tâm mới được thực hiện bước này.");

                var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(request.DocumentId, d => d.Versions);
                if (doc == null) return ApiResponse<bool>.Failure(404, "Tài liệu không tồn tại.");
                if (doc.Status != DocumentStatus.InternalApproved) return ApiResponse<bool>.Failure(400, "Tài liệu chưa ở bước Director duyệt.");

                var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);
                if (currentVersion == null) return ApiResponse<bool>.Failure(400, "Không tìm thấy phiên bản hiện hành.");

                var oldStatus = doc.Status;

                doc.Status = DocumentStatus.AppraisalPending;
                doc.CurrentHandlerId = null;
                _unitOfWork.TechnicalDocument.Update(doc);

                var history = new AppraisalHistory
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    RequestVersionId = currentVersion.Id,
                    HandlerId = userId,
                    OldStatus = oldStatus,
                    NewStatus = doc.Status,
                    Comment = request.Comment ?? "Giám đốc trung tâm duyệt → chuyển sang thẩm định đa bên.",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AppraisalHistory.AddAsync(history);

                var externalDeptIds = System.Text.Json.JsonSerializer
                    .Deserialize<List<Guid>>(doc.ExternalDepartmentIds ?? "[]") ?? new List<Guid>();

                if (!externalDeptIds.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                    return ApiResponse<bool>.Success(true, "Không có trung tâm thẩm định ngoài.");
                }

                foreach (var deptId in externalDeptIds)
                {
                    var director = await _unitOfWork.Users.GetDirectorByDepartmentAsync(deptId);
                    if (director == null) continue;

                    var assignment = new AppraisalAssignment
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = doc.Id,
                        DepartmentId = deptId,
                        ResponsibleManagerId = director.Id,
                        AssignedById = userId,
                        RequestVersionId = currentVersion.Id,
                        Status = AssignmentStatus.Pending
                    };

                    await _unitOfWork.AppraisalAssignments.AddAsync(assignment);

                    await _notificationService.SendNotificationWithRouteAsync(
                        NotificationRouteType.DirectorAssignment,
                        director.Id,
                        $"Yêu cầu thẩm định từ giám đốc: {currentUser.Profile.FullName} của trung tâm {currentUser.Department.NameDepartment}",
                        $"Tài liệu '{doc.Title}' đã được gửi đến trung tâm của bạn để thẩm định.",
                        doc.Id,
                        assignmentId: assignment.Id,
                        versionId: currentVersion.Id
                    );
                }

                //if (request.AttachmentIds?.Any() == true)
                //{
                //    var links = request.AttachmentIds.Select(attId => new AttachmentLink
                //    {
                //        AttachmentId = attId,
                //        EntityId = history.Id,
                //        EntityType = LinkedEntityType.AppraisalHistory
                //    });
                //    await _context.Set<AttachmentLink>().AddRangeAsync(links);
                //}
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Giám đốc trung tâm duyệt và đã gửi sang các trung tâm thẩm định.");
            });
        }

        public async Task<ApiResponse<bool>> RejectDocumentAsync(AppraisalRejectDto request, Guid userId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(request.DocumentId, d => d.Versions);
                if (doc == null) return ApiResponse<bool>.Failure(404, "Tài liệu không tồn tại.");

                var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);
                if (currentVersion == null) return ApiResponse<bool>.Failure(400, "Không có phiên bản hiện hành.");

                var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
                if (currentUser == null) return ApiResponse<bool>.Failure(404, "Tài khoản không tồn tại.");

                var isSenior = currentUser.Role == UserRole.DeputyInstituteDirector || currentUser.Role == UserRole.InstituteDirector;

                var oldStatus = doc.Status;
                doc.Status = DocumentStatus.Rejected;
                doc.CurrentHandlerId = doc.RequesterId;
                doc.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.TechnicalDocument.Update(doc);

                var history = new AppraisalHistory
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    RequestVersionId = currentVersion.Id,
                    HandlerId = userId,
                    OldStatus = oldStatus,
                    NewStatus = DocumentStatus.Rejected,
                    Comment = request.Comment ?? "Hồ sơ bị từ chối/bác bỏ.",
                    CreatedAt = DateTime.UtcNow
                };

                if (request.NewIssues?.Any() == true)
                {
                    var issues = request.NewIssues.Select(dto => PrepareFeedbackIssue(dto, currentVersion.Id, userId, history.Id));
                    await _unitOfWork.FeedbackIssues.AddRangeAsync(issues);
                }

                await _unitOfWork.AppraisalHistory.AddAsync(history);

                //if (request.AttachmentIds?.Any() == true)
                //{
                //    var links = request.AttachmentIds.Select(attId => new AttachmentLink
                //    {
                //        AttachmentId = attId,
                //        EntityId = history.Id,
                //        EntityType = LinkedEntityType.AppraisalHistory
                //    });
                //    await _context.Set<AttachmentLink>().AddRangeAsync(links);
                //}

                await _unitOfWork.SaveChangesAsync();

                var rejectMessage = isSenior
                    ? $"Tài liệu '{doc.Title}' đã bị bác bỏ và yêu cầu chỉnh sửa bổ sung bởi cấp viện do {currentUser.Profile.FullName} yêu cầu."
                    : $"Tài liệu '{doc.Title}' đã bị bác bỏ và yêu cầu chỉnh sửa bổ sung bởi cấp trung tâm do {currentUser.Profile.FullName} yêu cầu.";

                await _notificationService.SendNotificationWithRouteAsync(
                      NotificationRouteType.RejectedDocument,
                      doc.RequesterId,
                      $"Tài liệu {doc.Title} đã bị bác bỏ",
                      rejectMessage,
                      doc.Id,
                       versionId: currentVersion.Id);

                return ApiResponse<bool>.Success(true, "Đã bác bỏ hồ sơ thành công.");
            });
        }

        private FeedbackIssue PrepareFeedbackIssue(FeedbackIssueCreateDto dto, Guid requestVersionId, Guid reporterId, Guid? historyId = null)
        {
            var issue = _mapper.Map<FeedbackIssue>(dto);
            issue.ReporterId = reporterId;
            issue.AppraisalHistoryId = historyId;
            issue.RequestVersionId = requestVersionId;
            return issue;
        }

        // Đổi MemoryStream thành Stream ở đây
        public async Task<ApiResponse<(Stream Stream, string FileName)>> ExportVersionPdfAsync(Guid versionId)
        {
            var version = await _unitOfWork.RequestVersions.GetByIdAsync(versionId, v => v.Request, v => v.AppraisalHistories);

            if (version == null)
                return ApiResponse<(Stream, string)>.Failure(404, "Không tìm thấy phiên bản tài liệu.");

            if (version.Request.Status != DocumentStatus.Signing && version.Request.Status != DocumentStatus.Issued)
            {
                return ApiResponse<(Stream, string)>.Failure(400, "Chỉ xuất PDF cho tài liệu đã phê duyệt hoặc ban hành.");
            }

            var versionDetail = _mapper.Map<DocumentVersionDetailDto>(version);

            // GeneratePdfStreamAsync trả về MemoryStream (là con của Stream) -> Hợp lệ
            var pdfStream = await GeneratePdfStreamAsync(versionDetail, version.Request);
            pdfStream.Position = 0;

            string fileName = $"{version.Request.Title.Replace(" ", "_")}_V{version.VersionNumber}.pdf";

            // Trả về Tuple khớp với khai báo đầu hàm
            return ApiResponse<(Stream, string)>.Success((pdfStream, fileName));
        }

        private async Task<Stream> GeneratePdfStreamAsync(DocumentVersionDetailDto versionDto, TechnicalDocument doc)
        {
            var stream = new MemoryStream();
            return stream;
        }

        public async Task<ApiResponse<bool>> IssueDocumentAsync(Guid documentId, Guid userId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(documentId, d => d.Versions);
                if (doc == null) return ApiResponse<bool>.Failure(404, "The document does not exist.");
                if (doc.Status != DocumentStatus.Signing) return ApiResponse<bool>.Failure(400, "Only approved documents can be issued.");

                var currentVersion = doc.Versions.FirstOrDefault(v => v.IsCurrent);
                if (currentVersion == null) return ApiResponse<bool>.Failure(400, "Không tìm thấy phiên bản hiện hành.");

                var pendingDepts = await _unitOfWork.AppraisalAssignments
                  .GetAllAsQueryable()
                  .AnyAsync(a =>
                     a.DocumentId == documentId &&
                     a.Department.ParentId != null &&
                     (a.Status == AssignmentStatus.Pending || a.Status == AssignmentStatus.InReview)
              );

                if (pendingDepts) return ApiResponse<bool>.Failure(400, "Some units have yet to complete the assessment.");

                if (string.IsNullOrEmpty(doc.DocumentCode))
                {
                    doc.DocumentCode = $"VTS-{DateTime.UtcNow.Year}-{doc.Id.ToString().Substring(0, 8).ToUpper()}";
                }

                doc.QrCode = $"VTS-SIGN-{doc.DocumentCode}-{DateTime.UtcNow.Ticks}";
                doc.Status = DocumentStatus.Issued;

                await _unitOfWork.AppraisalHistory.AddAsync(new AppraisalHistory
                {
                    DocumentId = doc.Id,
                    RequestVersionId = currentVersion.Id,
                    HandlerId = userId,
                    OldStatus = DocumentStatus.Signing,
                    NewStatus = DocumentStatus.Issued,
                    Comment = $"Ban hành chính thức với mã số: {doc.DocumentCode}",
                    CreatedAt = DateTime.UtcNow
                });

                _unitOfWork.TechnicalDocument.Update(doc);
                await _unitOfWork.SaveChangesAsync();


                await _notificationService.SendNotificationWithRouteAsync(
                     NotificationRouteType.IssuedDocument,
                      doc.RequesterId,
                      $"Tài liệu {doc.Title}",
                      "Đã được ban hành",
                      doc.Id,
                      versionId: currentVersion.Id);

                return ApiResponse<bool>.Success(true, $"Tài liệu {doc.DocumentCode} đã được ban hành thành công.");
            });
        }
    }
}