using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Persistence
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryStaffDto> GetStaffDashboardSummaryAsync(Guid staffId);
        Task<DashboardSummaryManagerDto> GetManagerDashboardSummaryAsync(Guid managerId);
        Task<DepartmentDocumentStatusSummaryDto> GetDepartmentDocumentStatusSummaryAsync(Guid managerId);
        Task<DashboardSummaryDirectorDto> GetDirectorDashboardSummaryAsync(Guid directorId);
        Task<PagedResult<ManagerWorkloadDto>> GetManagerWorkloadsAsync(Guid directorId, PaginationDto pagination, string? searchTerm);
        Task<DashboardSummaryCoordinatorDto> GetCoordinatorDashboardSummaryAsync(Guid coordinatorId);
    }
}