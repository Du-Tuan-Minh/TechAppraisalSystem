using Application.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class TechnicalDocumentResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public IssueSeverity Priority { get; set; }
        public DocumentStatus Status { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? CurrentAssignmentId { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public Guid? CurrentReviewerId { get; set; }
    }

    public class TechnicalDocumentDetailDto : TechnicalDocumentResponseDto
    {
        public string? Description { get; set; }
        public string DocumentCode { get; set; } = string.Empty;
        public Guid RequesterId { get; set; }
        public Guid? CurrentHandlerId { get; set; }
        public string? CurrentHandlerName { get; set; }
        public Guid DepartmentId { get; set; }
        public int TotalVersions { get; set; }
        public string? QrCode { get; set; }
        public List<Guid>? ExternalDepartmentIds { get; set; }
        public List<Guid>? ApprovalProposerIds { get; set; }
    }

    public class TechnicalDocumentCreateDto
    {
        [Required(ErrorMessage = "The title document must not be blank.")]
        [MaxLength(255, ErrorMessage = "The title must not exceed 255 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã tài liệu là bắt buộc")]
        [MaxLength(100)]
        public string DocumentCode { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "The description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "This type of document is required.")]
        public DocumentType Type { get; set; }

        [Required]
        public IssueSeverity Priority { get; set; } = IssueSeverity.Minor;

        public List<Guid>? ExternalDepartmentIds { get; set; }
        public List<Guid>? ApprovalProposerIds { get; set; }
        public List<Guid>? AttachmentIds { get; set; }
        public Dictionary<string, object>? TechnicalSpecs { get; set; }
    }

    public class TechnicalDocumentUpdateDto
    {
        [Required(ErrorMessage = "Tiêu đề tài liệu không được để trống")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DocumentType Type { get; set; }

        [Required]
        public IssueSeverity Priority { get; set; }
        public Dictionary<string, object>? TechnicalSpecs { get; set; }
        public List<Guid>? ExternalDepartmentIds { get; set; }
        public Guid? ApprovalProposerIds { get; set; }
        public string? ChangeReason { get; set; }
    }

    public class DocumentFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public DocumentType? Type { get; set; }
        public Guid? DepartmentId { get; set; }
        public DocumentStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class UserCurrentDocumentDto
    {
        public Guid Id { get; set; }
        public Guid VersionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public IssueSeverity Priority { get; set; }
        public Guid? CurrentHandlerId { get; set; }
        public string? CurrentHandlerName { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class UserCurrentDocumentFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public IssueSeverity? Priority { get; set; }
        public StaffDashboardDocumentType? Type { get; set; }
    }

    public class PendingAppraisalResponseDto
    {
        public Guid AssignmentId { get; set; }
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public Guid DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public ReviewerStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class PendingAppraisalFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public ReviewerStatus? Status { get; set; }
    }

    public class OverdueDocumentDto
    {
        public Guid AssignmentId { get; set; }
        public Guid ReviewerId { get; set; }

        public string ReviewerName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;

        public Guid DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public Guid RequesterId { get; set; }
        public string RequesterName { get; set; } = string.Empty;

        public ReviewerStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class OverdueFilterDto : PaginationDto
    {
        public string? SearchTerm { get; set; }
        public ReviewerStatus? Status { get; set; }
    }

    public class ManagerDashboardDocumentFilterDto : PaginationDto
    {
        public ManagerDashboardDocumentType? Type { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class DepartmentDocumentStatusFilterDto : PaginationDto
    {
        public DepartmentDocumentStatusType? Type { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class ManagerDashboardDocumentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public string? CurrentHandlerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? AssignmentId { get; set; }
        public Guid? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public AssignmentStatus? AssignmentStatus { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class IncomingAppraisalDocumentDto
    {
        public Guid Id { get; set; }
        public Guid VersionId { get; set; }
        public int VersionNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}