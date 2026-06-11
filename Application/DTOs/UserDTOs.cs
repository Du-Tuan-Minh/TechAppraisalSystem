using Application.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UserDetailResponseDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class UserUpdateAccountDto
    {
        public bool? IsActive { get; set; }
        public UserRole? Role { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [MaxLength(255)]
        public string? FirstName { get; set; }

        [MaxLength(255)]
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class UserFilterDto : PaginationDto
    {
        public Guid? DepartmentId { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class DocumentTypeStatisticDto
    {
        public DocumentType Type { get; set; }
        public int TotalDocuments { get; set; }
    }

    public class UserAppraisalAssigneeDto
    {
        public Guid DepartmentId { get; set; }
        public Guid UserId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int TotalDocuments { get; set; }
    }
}