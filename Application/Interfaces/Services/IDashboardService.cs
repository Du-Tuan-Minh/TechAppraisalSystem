using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryStaffDto>> GetStaffDashboardSummaryAsync(Guid staffId);
        Task<ApiResponse<DashboardSummaryManagerDto>> GetManagerDashboardSummaryAsync(Guid managerId);
        Task<ApiResponse<DepartmentDocumentStatusSummaryDto>> GetDepartmentDocumentStatusSummaryAsync(Guid managerId);
        Task<ApiResponse<DashboardSummaryDirectorDto>> GetDirectorDashboardSummaryAsync(Guid directorId);
        Task<ApiResponse<PagedResult<ManagerWorkloadDto>>> GetManagerWorkloadsAsync(Guid directorId, PaginationDto pagination, string? searchTerm);
        Task<ApiResponse<DashboardSummaryCoordinatorDto>> GetCoordinatorDashboardSummaryAsync(Guid coordinatorId);
    }
}