using Application.Common;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System.Text.Json;
using AutoMapperProfile = AutoMapper.Profile;
using EntityProfile = Domain.Entities.Profile;

namespace Application.Mappings
{
    public class MappingProfile : AutoMapperProfile
    {
        public MappingProfile()
        {
            CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));

            CreateMap<Guid?, Guid?>().ConvertUsing(src => (src == Guid.Empty) ? null : src);

            // USER & PROFILE
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.FirstName : null))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.LastName : null))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.NameDepartment : "Chưa có phòng ban"));

            CreateMap<User, UserDetailResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.FirstName : null))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.LastName : null))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.AvatarUrl : null))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Profile != null ? src.Profile.PhoneNumber : null))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.NameDepartment : "Chưa có phòng ban"));

            CreateMap<UpdateProfileDto, EntityProfile>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            //CreateMap<User, DocumentTypeStatisticDto>()
            //    .ForMember(dest => dest.FullName,
            //        opt => opt.MapFrom(src =>
            //            src.Profile != null
            //                ? $"{src.Profile.FirstName ?? ""} {src.Profile.LastName ?? ""}".Trim()
            //                : string.Empty))
            //    .ForMember(dest => dest.TotalDocuments, opt => opt.Ignore());

            CreateMap<UserUpdateAccountDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // TECHNICAL DOCUMENT & VERSIONS
            CreateMap<TechnicalDocument, TechnicalDocumentResponseDto>()
                .ForMember(d => d.RequesterName, o => o.MapFrom(s => s.Requester.Profile != null ? s.Requester.Profile.FullName : s.Requester.EmployeeCode))
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.NameDepartment : "N/A"))
                .ForMember(d => d.CurrentAssignmentId, o => o.Ignore())
                .ForMember(d => d.CurrentReviewerId, o => o.Ignore())
                .ForMember(d => d.CurrentVersionId, o => o.MapFrom(s => s.Versions.FirstOrDefault(v => v.IsCurrent).Id));

            CreateMap<TechnicalDocument, TechnicalDocumentDetailDto>()
                .IncludeBase<TechnicalDocument, TechnicalDocumentResponseDto>()
                .ForMember(d => d.ExternalDepartmentIds, o => o.MapFrom(s =>
                    string.IsNullOrEmpty(s.ExternalDepartmentIds)
                    ? new List<Guid>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(s.ExternalDepartmentIds, (System.Text.Json.JsonSerializerOptions)null)))
                .ForMember(d => d.ApprovalProposerIds, o => o.MapFrom(s =>
                    string.IsNullOrEmpty(s.ApprovalProposerIds)
                    ? new List<Guid>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(s.ApprovalProposerIds, (System.Text.Json.JsonSerializerOptions)null)))
                .ForMember(d => d.TotalVersions, o => o.MapFrom(s => s.Versions.Count))
                .ForMember(d => d.CurrentHandlerName, o => o.MapFrom(s => s.CurrentHandler != null
                    ? (s.CurrentHandler.Profile != null ? s.CurrentHandler.Profile.FullName : s.CurrentHandler.EmployeeCode)
                    : "Chưa có người xử lý"));

            CreateMap<TechnicalDocumentCreateDto, TechnicalDocument>()
                .ForMember(d => d.ExternalDepartmentIds, o => o.MapFrom(s =>
                    System.Text.Json.JsonSerializer.Serialize(s.ExternalDepartmentIds ?? new List<Guid>(), (System.Text.Json.JsonSerializerOptions)null)))
                 .ForMember(d => d.ApprovalProposerIds, o => o.MapFrom(s =>
                    System.Text.Json.JsonSerializer.Serialize(s.ApprovalProposerIds ?? new List<Guid>(), (System.Text.Json.JsonSerializerOptions)null)))
                .ForMember(d => d.Status, o => o.MapFrom(_ => DocumentStatus.Draft))
                .ForMember(d => d.Attachments, o => o.Ignore())
                .ForSourceMember(s => s.TechnicalSpecs, o => o.DoNotValidate());

            CreateMap<RequestVersion, DocumentVersionDto>()
                .ForMember(dest => dest.TechnicalSpecsJson, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.TechnicalSpecsJson)
                    ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(
                    src.TechnicalSpecsJson,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));

            CreateMap<TechnicalDocumentCreateDto, RequestVersion>()
                .ForMember(d => d.TechnicalSpecsJson, o => o.MapFrom(s =>
                    System.Text.Json.JsonSerializer.Serialize(s.TechnicalSpecs, (System.Text.Json.JsonSerializerOptions)null)));

            CreateMap<RequestVersion, DocumentVersionDetailDto>()
                .IncludeBase<RequestVersion, DocumentVersionDto>();

            //CreateMap<DocumentVersionCreateDto, RequestVersion>()
            //    .ForMember(d => d.TechnicalSpecsJson, o => o.MapFrom(s =>
            //        System.Text.Json.JsonSerializer.Serialize(s.TechnicalSpecsJson, (System.Text.Json.JsonSerializerOptions)null)))
            //    .ForMember(d => d.IsCurrent, o => o.MapFrom(_ => true))
            //    .ForMember(d => d.VersionNumber, o => o.Ignore());

            CreateMap<TechnicalDocumentUpdateDto, TechnicalDocument>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<TechnicalDocumentUpdateDto, RequestVersion>()
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<RequestVersion, RequestVersion>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.IsCurrent, o => o.MapFrom(_ => true))
                .ForMember(d => d.AppraisalHistories, o => o.Ignore());

            CreateMap<TechnicalDocument, UserCurrentDocumentDto>()
                .ForMember(d => d.VersionId, o => o.MapFrom(s => s.Versions.FirstOrDefault(v => v.IsCurrent)!.Id))
                .ForMember(d => d.CurrentHandlerName, o => o.MapFrom(s => s.CurrentHandler!.Profile!.FullName))
                .ForMember(d => d.Deadline, o => o.MapFrom(s => s.AppraisalAssignments.FirstOrDefault(a => a.Version.IsCurrent)!.Deadline));

            CreateMap<TechnicalDocument, ManagerDashboardDocumentDto>()
                .ForMember(d => d.CurrentHandlerName, o => o.MapFrom(s =>
                    s.CurrentHandler != null
                        ? (s.CurrentHandler.Profile != null
                            ? s.CurrentHandler.Profile.FullName
                            : s.CurrentHandler.EmployeeCode)
                        : null))
                .ForMember(d => d.AssignmentId, o => o.MapFrom(s =>
                    s.AppraisalAssignments
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefault() != null
                            ? s.AppraisalAssignments
                                .OrderByDescending(a => a.CreatedAt)
                                .FirstOrDefault()!.Id
                            : (Guid?)null))
                .ForMember(d => d.AssignmentStatus, o => o.MapFrom(s =>
                    s.AppraisalAssignments
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefault() != null
                            ? s.AppraisalAssignments
                                .OrderByDescending(a => a.CreatedAt)
                                .FirstOrDefault()!.Status
                            : (AssignmentStatus?)null))
                .ForMember(d => d.Deadline, o => o.MapFrom(s =>
                    s.AppraisalAssignments
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefault() != null
                            ? s.AppraisalAssignments
                                .OrderByDescending(a => a.CreatedAt)
                                .FirstOrDefault()!.Deadline
                            : null))
                .ForMember(d => d.ReviewerId, o => o.MapFrom(s =>
                    s.AppraisalAssignments
                        .SelectMany(a => a.Reviewers)
                        .OrderByDescending(r => r.CreatedAt)
                        .FirstOrDefault() != null
                            ? s.AppraisalAssignments
                                .SelectMany(a => a.Reviewers)
                                .OrderByDescending(r => r.CreatedAt)
                                .FirstOrDefault()!.Id
                            : (Guid?)null))
                .ForMember(d => d.ReviewerName, o => o.MapFrom(s =>
                    s.AppraisalAssignments
                        .SelectMany(a => a.Reviewers)
                        .OrderByDescending(r => r.CreatedAt)
                        .FirstOrDefault() != null
                            ? (
                                s.AppraisalAssignments
                                    .SelectMany(a => a.Reviewers)
                                    .OrderByDescending(r => r.CreatedAt)
                                    .FirstOrDefault()!.Staff.Profile != null
                                        ? s.AppraisalAssignments
                                            .SelectMany(a => a.Reviewers)
                                            .OrderByDescending(r => r.CreatedAt)
                                            .FirstOrDefault()!.Staff.Profile!.FullName
                                        : s.AppraisalAssignments
                                            .SelectMany(a => a.Reviewers)
                                            .OrderByDescending(r => r.CreatedAt)
                                            .FirstOrDefault()!.Staff.EmployeeCode
                              )
                            : null));

            CreateMap<TechnicalDocument, IncomingAppraisalDocumentDto>()
                 .ForMember(
                     dest => dest.VersionId,
                     opt => opt.MapFrom(src =>
                         src.Versions
                             .Where(v => v.IsCurrent)
                             .Select(v => v.Id)
                             .FirstOrDefault()))
                 .ForMember(dest => dest.VersionNumber, opt => opt.MapFrom(src =>
                         src.Versions
                             .Where(v => v.IsCurrent)
                             .Select(v => v.VersionNumber)
                             .FirstOrDefault()));

            // HISTORY
            CreateMap<AppraisalHistory, AppraisalHistoryResponseDto>()
                .ForMember(dest => dest.HandlerName, opt => opt.MapFrom(src =>
                    src.Handler.Profile != null ? src.Handler.Profile.FullName : src.Handler.EmployeeCode))

                .ForMember(dest => dest.DocumentTitle, opt => opt.MapFrom(src => src.Document.Title))
                .ForMember(dest => dest.VersionNumber, opt => opt.MapFrom(src =>
                    src.RequestVersion != null ? src.RequestVersion.VersionNumber : 0))

                .ForMember(dest => dest.LinkedIssues, opt => opt.MapFrom(src => src.FeedbackIssues));

            CreateMap<AppraisalRejectDto, AppraisalHistory>()
                .ForMember(dest => dest.OldStatus, opt => opt.Ignore())
                .ForMember(dest => dest.FeedbackIssues, opt => opt.Ignore());

            // NOTIFICATION
            CreateMap<UserNotification, UserNotificationResponseDto>()
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Notification.Title))
                .ForMember(d => d.Content, o => o.MapFrom(s => s.Notification.Content))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Notification.Type))
                .ForMember(d => d.Metadata, o => o.MapFrom(s =>
                    !string.IsNullOrEmpty(s.Notification.Metadata)
                        ? JsonSerializer.Deserialize<object>(s.Notification.Metadata, JsonSerializerOptions.Default)
                        : null))
                .ForMember(d => d.SenderId, o => o.MapFrom(s => s.Notification.SenderId))
                .ForMember(d => d.SenderName, o => o.MapFrom(s =>
                    s.Notification.Sender != null
                        ? (s.Notification.Sender.Profile != null ? s.Notification.Sender.Profile.FullName : s.Notification.Sender.EmployeeCode)
                        : "Hệ thống"))
                .ForMember(d => d.SenderAvatar, o => o.MapFrom(s =>
                    s.Notification.Sender != null && s.Notification.Sender.Profile != null ? s.Notification.Sender.Profile.AvatarUrl : null))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.Notification.CreatedAt));

            CreateMap<NotificationCreateDto, Notification>()
                .ForMember(d => d.Metadata, o => o.MapFrom(s =>
                    s.Metadata != null ? JsonSerializer.Serialize(s.Metadata, JsonSerializerOptions.Default) : null))
                .ForMember(d => d.UserNotifications, o => o.Ignore());

            // KNOWLEDGE BASE
            CreateMap<TechnicalKnowledgeBase, TechnicalKnowledgeBaseDetailDto>()
                .ForMember(dest => dest.LinkedSpecPattern, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.LinkedSpecPattern)
                        ? new object()
                        : System.Text.Json.JsonSerializer.Deserialize<object>(src.LinkedSpecPattern, (System.Text.Json.JsonSerializerOptions)null)))
                .ForMember(dest => dest.VerifiedByName, opt => opt.MapFrom(src =>
                    src.Verifier != null && src.Verifier.Profile != null
                        ? src.Verifier.Profile.FullName
                        : (src.Verifier != null ? src.Verifier.EmployeeCode : null)));

            CreateMap<TechnicalKnowledgeBase, TechnicalKnowledgeBaseResponseDto>();

            CreateMap<TechnicalKnowledgeBaseCreateDto, TechnicalKnowledgeBase>()
                .ForMember(dest => dest.LinkedSpecPattern, opt => opt.MapFrom(src =>
                    System.Text.Json.JsonSerializer.Serialize(src.LinkedSpecPattern, (System.Text.Json.JsonSerializerOptions)null)))
                .ForMember(dest => dest.OccurrenceCount, opt => opt.MapFrom(_ => 1))
                .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(_ => false));

            CreateMap<TechnicalKnowledgeBaseUpdateDto, TechnicalKnowledgeBase>()
                .ForMember(dest => dest.LinkedSpecPattern, opt => opt.Condition(src => src.LinkedSpecPattern != null))
                .ForMember(dest => dest.LinkedSpecPattern, opt => opt.MapFrom(src =>
                    System.Text.Json.JsonSerializer.Serialize(src.LinkedSpecPattern, (System.Text.Json.JsonSerializerOptions)null)))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ATTACHMENTS
            CreateMap<AttachmentLink, AttachmentLinkDto>();

            CreateMap<Attachment, AttachmentResponseDto>()
                .ForMember(dest => dest.UploaderName, opt => opt.MapFrom(src =>
                    src.Uploader.Profile != null ? src.Uploader.Profile.FullName : src.Uploader.EmployeeCode))
                .ForMember(dest => dest.Links, opt => opt.MapFrom(src => src.Links));

            CreateMap<AttachmentCreateDto, Attachment>()
                .ForMember(dest => dest.FileData, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.FileSize, opt => opt.Ignore())
                .ForMember(dest => dest.FileType, opt => opt.Ignore())
                .ForMember(dest => dest.Links, opt => opt.Ignore());

            //FEEDBACK ISSUE
            CreateMap<FeedbackIssueCreateDto, FeedbackIssue>()
                .ForMember(d => d.Status, o => o.MapFrom(_ => IssueStatus.New))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.ReporterId, o => o.Ignore());

            CreateMap<FeedbackIssue, FeedbackIssueResponseDto>()
                .ForMember(d => d.VersionNumber, o => o.MapFrom(s => s.Version != null ? s.Version.VersionNumber : 0))
                .ForMember(d => d.DocumentTitle, o => o.MapFrom(s =>
                    (s.Version != null && s.Version.Request != null) ? s.Version.Request.Title : "N/A"))
                .ForMember(d => d.AssignedDepartmentName, o => o.MapFrom(s =>
                    s.AssignedDepartment != null ? s.AssignedDepartment.NameDepartment : null));

            CreateMap<FeedbackIssue, FeedbackIssueDetailDto>()
                .IncludeBase<FeedbackIssue, FeedbackIssueResponseDto>()
                .ForMember(d => d.ReporterName, o => o.MapFrom(s =>
                    s.Reporter != null && s.Reporter.Profile != null
                        ? s.Reporter.Profile.FullName
                        : (s.Reporter != null ? s.Reporter.EmployeeCode : "N/A")))
                .ForMember(d => d.KnowledgeBaseTitle, o => o.MapFrom(s =>
                    s.TechnicalKnowledgeBase != null ? s.TechnicalKnowledgeBase.Title : null))
                .ForMember(d => d.AttachmentCount, o => o.MapFrom(s => s.AttachmentLinks.Count))
                .ForMember(d => d.Attachments, o => o.Ignore());

            CreateMap<FeedbackIssueUpdateDto, FeedbackIssue>()
                .ForAllMembers(o => o.Condition((src, dest, member) => member != null));

            //CreateMap<FeedbackActionRequestDto, FeedbackIssue>()
            //    .ForMember(d => d.Id, o => o.MapFrom(s => s.IssueId))
            //    .ForMember(d => d.Description, o => o.MapFrom(s => s.Content))
            //    .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<FeedbackResponseDto, AppraisalHistory>(MemberList.None)
                .ForMember(d => d.Comment, o => o.MapFrom(s => s.Comment));

            //DEPARTMENT
            CreateMap<Department, DepartmentResponseDto>()
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Users
                    .Where(u => u.Role == UserRole.Manager && u.IsActive)
                    .Select(u => u.Profile != null ? u.Profile.FullName : u.EmployeeCode)
                    .FirstOrDefault()))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.Users
                    .Where(u => u.Role == UserRole.Manager && u.IsActive)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefault()));

            CreateMap<DepartmentCreateDto, Department>();

            CreateMap<DepartmentUpdateDto, Department>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<DepartmentInvitation, DepartmentInvitationResponseDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.NameDepartment))
                .ForMember(dest => dest.InviterName, opt => opt.Ignore());

            CreateMap<DepartmentInvitationCreateDto, DepartmentInvitation>()
                .ForMember(dest => dest.InviteeEmployeeCode, opt => opt.MapFrom(src => src.EmployeeCode))
                .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(_ => DateTime.UtcNow.AddDays(7)))
                .ForMember(dest => dest.InvitationCode, opt => opt.Ignore())
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(_ => false));

            // APPRAISAL ASSIGNMENT (Manager Level) 
            CreateMap<AppraisalAssignment, AppraisalAssignmentDto>()
                .ForMember(d => d.DocumentTitle, o => o.MapFrom(s => s.Document.Title))
                .ForMember(d => d.DocumentCode, o => o.MapFrom(s => s.Document.DocumentCode))
                .ForMember(d => d.VersionNumber, o => o.MapFrom(s => s.Version.VersionNumber))
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.NameDepartment))
                .ForMember(d => d.DepartmentId, o => o.MapFrom(s => s.Department.Id))
                .IncludeAllDerived();

            CreateMap<AppraisalAssignment, AppraisalAssignmentDetailDto>()
                .ForMember(d => d.DocumentCode, o => o.MapFrom(s => s.Document.DocumentCode))
                .ForMember(d => d.AssignedByName, o => o.MapFrom(s =>
                    s.AssignedBy.Profile != null ? s.AssignedBy.Profile.FullName : s.AssignedBy.EmployeeCode))
                .ForMember(d => d.ResponsibleManagerName, o => o.MapFrom(s =>
                    s.ResponsibleManager.Profile != null ? s.ResponsibleManager.Profile.FullName : s.ResponsibleManager.EmployeeCode))
                .ForMember(d => d.ReviewerCount, o => o.MapFrom(s => s.Reviewers.Count))
                .ForMember(d => d.IssueCount, o => o.MapFrom(s => s.FeedbackIssues.Count))
                .ForMember(d => d.AttachmentCount, o => o.Ignore());

            CreateMap<DepartmentAssignmentInfo, AppraisalAssignment>()
                .ForMember(d => d.Status, o => o.MapFrom(_ => AssignmentStatus.Pending))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

            CreateMap<AppraisalReviewer, AppraisalReviewerDto>()
                .ForMember(d => d.StaffName, o => o.MapFrom(s =>
                    s.Staff.Profile != null ? s.Staff.Profile.FullName : s.Staff.EmployeeCode));
            //.ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Assignment.Department.NameDepartment))
            //.ForMember(d => d.IssueCount, o => o.MapFrom(s => s.Assignment.FeedbackIssues.Count(i => i.ReporterId == s.StaffId)))
            //.ForMember(d => d.AttachmentCount, o => o.Ignore());

            CreateMap<UpdateReviewerProgressRequest, AppraisalReviewer>()
                .ForMember(d => d.CompletedAt, o => o.MapFrom(s =>
                    s.Status == ReviewerStatus.Completed ? DateTime.UtcNow : (DateTime?)null))
                .ForAllMembers(o => o.Condition((src, dest, member) => member != null));

            CreateMap<AppraisalReviewer, PendingAppraisalResponseDto>()
                .ForMember(d => d.ReviewerId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.ReviewerName,
                    o => o.MapFrom(s =>
                        s.Staff.Profile != null
                            ? s.Staff.Profile.FullName
                            : s.Staff.EmployeeCode))

                .ForMember(d => d.EmployeeCode, o => o.MapFrom(s => s.Staff.EmployeeCode))
                .ForMember(d => d.DocumentId, o => o.MapFrom(s => s.Assignment.DocumentId))
                .ForMember(d => d.DocumentTitle, o => o.MapFrom(s => s.Assignment.Document.Title))
                .ForMember(d => d.DocumentCode, o => o.MapFrom(s => s.Assignment.Document.DocumentCode));

            CreateMap<AppraisalReviewer, OverdueDocumentDto>()
                .ForMember(d => d.ReviewerId,
                    o => o.MapFrom(s => s.Id))
                .ForMember(d => d.ReviewerName,
                    o => o.MapFrom(s =>
                        s.Staff.Profile != null
                            ? s.Staff.Profile.FullName
                            : s.Staff.EmployeeCode))
                .ForMember(d => d.EmployeeCode,
                    o => o.MapFrom(s => s.Staff.EmployeeCode))
                .ForMember(d => d.DocumentId,
                    o => o.MapFrom(s => s.Assignment.DocumentId))
                .ForMember(d => d.DocumentTitle,
                    o => o.MapFrom(s => s.Assignment.Document.Title))
                .ForMember(d => d.DocumentCode,
                    o => o.MapFrom(s => s.Assignment.Document.DocumentCode))
                .ForMember(d => d.RequesterId,
                    o => o.MapFrom(s => s.Assignment.Document.RequesterId))
                .ForMember(d => d.RequesterName,
                    o => o.MapFrom(s =>
                        s.Assignment.Document.Requester.Profile != null
                            ? s.Assignment.Document.Requester.Profile.FullName
                            : s.Assignment.Document.Requester.EmployeeCode));

            CreateMap<User, UserAppraisalAssigneeDto>()
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s =>
                    s.Department != null
                        ? s.Department.NameDepartment
                        : string.Empty))
                .ForMember(d => d.FullName, o => o.MapFrom(s =>
                    s.Profile != null
                        ? s.Profile.FullName
                        : s.EmployeeCode))
                .ForMember(d => d.TotalDocuments, o => o.Ignore());

            // APPROVAL WORKFLOW
            CreateMap<ApprovalWorkflow, ApprovalWorkflowResponseDto>()
                .ForMember(d => d.ApproverName, o => o.MapFrom(s =>
                    s.Approver != null
                        ? (s.Approver.Profile != null ? s.Approver.Profile.FullName : s.Approver.EmployeeCode)
                        : null))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status));

            CreateMap<ApprovalWorkflow, ApprovalWorkflowDetailDto>().IncludeBase<ApprovalWorkflow, ApprovalWorkflowResponseDto>();

            CreateMap<CreateWorkflowRequest, ApprovalWorkflow>()
                .ForMember(d => d.Status, o => o.MapFrom(_ => AssignmentStatus.Pending))
                .ForMember(d => d.IsCurrentStep, o => o.MapFrom(_ => false));

            CreateMap<WorkflowStepConfigDto, ApprovalWorkflow>()
                .ForMember(d => d.Status, o => o.MapFrom(_ => AssignmentStatus.Pending))
                .ForMember(d => d.IsCurrentStep, o => o.MapFrom(_ => false));

            CreateMap<ApprovalActionRequestDto, ApprovalWorkflow>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.ApprovalWorkflowId))
                .ForMember(d => d.SignedAt, o => o.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.Status, o => o.Ignore())
                .ForAllMembers(o => o.Condition((src, dest, member) => member != null));

            // FEEDBACK COMMENT
            CreateMap<FeedbackComment, FeedbackCommentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User.Profile != null ? src.User.Profile.FullName : src.User.EmployeeCode))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src =>
                    src.User.Profile != null ? src.User.Profile.AvatarUrl : null))
                // Map danh sách con (Replies) - EF Core sẽ tự handle nếu bạn Include trong Service
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies))
                // Map đính kèm thông qua bảng trung gian AttachmentLink
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src =>
                    src.AttachmentLinks.Select(al => al.Attachment)));

            CreateMap<CreateFeedbackCommentRequest, FeedbackComment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.AttachmentLinks, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore())
                .ForMember(dest => dest.FeedbackIssue, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore());


            // WORKFLOW PROGRESS
            CreateMap<ApprovalWorkflow, ApprovalStepResponseDto>()
                .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src =>
                    src.Approver != null
                        ? (src.Approver.Profile != null ? src.Approver.Profile.FullName : src.Approver.EmployeeCode)
                        : "Chưa chỉ định"));

            CreateMap<ApprovalWorkflow, SigningWorkflowResponseDto>()
                .ForMember(dest => dest.WorkflowSteps, opt => opt.Ignore())
                .ForMember(dest => dest.Reviewers, opt => opt.Ignore());

            CreateMap<AppraisalReviewer, ReviewerInfoDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    src.Staff.Profile != null ? src.Staff.Profile.FullName : src.Staff.EmployeeCode))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Staff.Role.ToString()))
                .ForMember(dest => dest.DepartmentName, opt => opt.Ignore());

            CreateMap<AppraisalHistory, WorkflowVersionEventDto>()
                .ForMember(dest => dest.ActorName, opt => opt.MapFrom(src =>
                    src.Handler.Profile != null ? src.Handler.Profile.FullName : src.Handler.EmployeeCode))
                .ForMember(dest => dest.ActorRole, opt => opt.MapFrom(src => src.Handler.Role));

            CreateMap<TechnicalKnowledgeBase, SuggestionResponseDto>()
                .ForMember(dest => dest.SourceTitle, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.LinkedSpecPattern, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.LinkedSpecPattern)
                       ? (JsonElement?)null
                       : JsonSerializer.Deserialize<JsonElement>(src.LinkedSpecPattern, (JsonSerializerOptions)null)))
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            CreateMap<Attachment, AttachmentShortDto>();

            CreateMap<AttachmentLink, AttachmentShortDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AttachmentId))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.Attachment.FileName))
                .ForMember(dest => dest.ContentCategory, opt => opt.MapFrom(src => src.Attachment.ContentCategory));
        }
    }
}