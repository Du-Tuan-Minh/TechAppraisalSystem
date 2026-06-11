using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class DepartmentResponseDto
    {
        public Guid Id { get; set; }
        public string NameDepartment { get; set; } = string.Empty;
        public string CodeDepartment { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ManagerName { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DepartmentCreateDto
    {
        [Required]
        public string NameDepartment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã đơn vị là bắt buộc")]
        [MaxLength(50)]
        public string CodeDepartment { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class DepartmentInvitationResponseDto
    {
        public Guid Id { get; set; }
        public string InviteeEmployeeCode { get; set; } = string.Empty;
        public string InvitationCode { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string InviterName { get; set; } = string.Empty;
    }

    public class DepartmentInvitationCreateDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
    }

    public class DepartmentUpdateDto
    {
        [Required]
        public string NameDepartment { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentId { get; set; }
    }
}