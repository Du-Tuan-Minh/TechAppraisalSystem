using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IDepartmentService
    {
        Task<ApiResponse<string>> InviteToDepartmentAsync(Guid managerId, DepartmentInvitationCreateDto invite);
        Task<ApiResponse<bool>> JoinDepartmentAsync(Guid userId, string inviteCode);
        Task<ApiResponse<DepartmentResponseDto>> CreateDepartmentAsync(DepartmentCreateDto dto, Guid userId);
        Task<ApiResponse<DepartmentResponseDto>> UpdateDepartmentAsync(Guid id, Guid userId, DepartmentUpdateDto dto);
        Task<ApiResponse<bool>> DeleteDepartmentAsync(Guid id, Guid userId);
        Task<ApiResponse<PagedResult<DepartmentResponseDto>>> GetCentersAsync(PaginationDto pagination, string? searchTerm);
        Task<ApiResponse<PagedResult<DepartmentResponseDto>>> GetSubDepartmentsAsync(Guid userId, Guid centerId, PaginationDto pagination, string? searchTerm);
    }
}