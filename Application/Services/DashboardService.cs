using Application.Common;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;

namespace Application.Services
{
    public class DashboardService : BaseService, IDashboardService
    {

        private readonly IMapper _mapper;

        public DashboardService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public async Task<ApiResponse<DashboardSummaryStaffDto>> GetStaffDashboardSummaryAsync(Guid staffId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var summary = await _unitOfWork.Dashboard.GetStaffDashboardSummaryAsync(staffId);
                return ApiResponse<DashboardSummaryStaffDto>.Success(summary);
            });
        }

        public async Task<ApiResponse<DashboardSummaryManagerDto>> GetManagerDashboardSummaryAsync(Guid managerId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var result = await _unitOfWork.Dashboard.GetManagerDashboardSummaryAsync(managerId);
                return ApiResponse<DashboardSummaryManagerDto>.Success(result);
            });
        }

        public async Task<ApiResponse<DepartmentDocumentStatusSummaryDto>> GetDepartmentDocumentStatusSummaryAsync(Guid managerId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var result = await _unitOfWork.Dashboard.GetDepartmentDocumentStatusSummaryAsync(managerId);
                return ApiResponse<DepartmentDocumentStatusSummaryDto>.Success(result);
            });
        }

        public async Task<ApiResponse<DashboardSummaryDirectorDto>> GetDirectorDashboardSummaryAsync(Guid directorId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var result = await _unitOfWork.Dashboard.GetDirectorDashboardSummaryAsync(directorId);
                return ApiResponse<DashboardSummaryDirectorDto>.Success(result);
            });
        }

        public async Task<ApiResponse<PagedResult<ManagerWorkloadDto>>> GetManagerWorkloadsAsync(Guid directorId, PaginationDto pagination, string? searchTerm)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var result = await _unitOfWork.Dashboard.GetManagerWorkloadsAsync(directorId, pagination, searchTerm);
                return ApiResponse<PagedResult<ManagerWorkloadDto>>.Success(result);
            });
        }

        public async Task<ApiResponse<DashboardSummaryCoordinatorDto>> GetCoordinatorDashboardSummaryAsync(Guid coordinatorId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var result = await _unitOfWork.Dashboard.GetCoordinatorDashboardSummaryAsync(coordinatorId);
                return ApiResponse<DashboardSummaryCoordinatorDto>.Success(result);
            });
        }
    }
}