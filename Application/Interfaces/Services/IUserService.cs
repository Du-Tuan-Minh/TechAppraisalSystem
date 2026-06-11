using Application.Common;
using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<ApiResponse<bool>> UpdateUserAccountAsync(Guid userId, UserUpdateAccountDto dto);
        Task<ApiResponse<UserDetailResponseDto>> GetMyProfileAsync(Guid userId);
        Task<ApiResponse<bool>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task<ApiResponse<bool>> RequestRolePromotionAsync(Guid userId, UserRole requestedRole, string reason);
        Task<ApiResponse<PagedResult<UserResponseDto>>> GetUsersPagedAsync(UserFilterDto filter, Guid currentUserId);
        Task<ApiResponse<UserDetailResponseDto>> GetUserDetailAsync(Guid userId);
        Task<ApiResponse<PagedResult<UserResponseDto>>> GetSeniorCenterAsync(PaginationDto pagination, string? searchTerm);
        //Task<ApiResponse<PagedResult<DocumentTypeStatisticDto>>> GetTopDocumentAuthorsAsync(Guid managerId, PaginationDto pagination, string? searchTerm);
        Task<ApiResponse<PagedResult<DocumentTypeStatisticDto>>> GetTopRejectedDocumentsAsync(Guid managerId, PaginationDto pagination);
        Task<ApiResponse<PagedResult<UserAppraisalAssigneeDto>>> GetDepartmentAppraisalWorkloadsAsync(PaginationDto pagination, string? searchTerm);
    }
}