using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAppraisalService
    {
        Task<ApiResponse<bool>> CreateParallelAssignmentsAsync(CreateParallelAssignmentsRequest request, Guid senderId);
        Task<ApiResponse<bool>> ConfirmCenterResultAsync(ConsolidateAppraisalRequest request, Guid ManagerId);
        Task<ApiResponse<bool>> AssignInternalStaffAsync(AssignStaffRequest request, Guid managerId);
        Task<ApiResponse<bool>> ConfirmDepartmentResultAsync(Guid documentId, CompleteAssignmentRequest request, Guid managerId);
        Task<ApiResponse<bool>> SubmitStaffReviewAsync(Guid reviewerId, UpdateReviewerProgressRequest dto, Guid staffId);
        Task<ApiResponse<AppraisalAssignmentDetailDto>> GetAssignmentDetailAsync(Guid assignmentId);
        Task<ApiResponse<PagedResult<AppraisalAssignmentDto>>> GetAssignmentsForDirectorAsync(Guid directorId, PaginationDto pagination);
        Task<ApiResponse<PagedResult<AppraisalAssignmentDto>>> GetAssignmentsForManagerAsync(Guid managerId, Guid? versionId, PaginationDto pagination);
        Task<ApiResponse<AppraisalReviewerDetailDto>> GetReviewerDetailAsync(Guid reviewerId);
        Task<ApiResponse<PagedResult<AppraisalReviewerDto>>> GetAssignmentsReviewForStaffAsync(Guid? assignmentId, Guid staffId, PaginationDto pagination);
        Task<ApiResponse<bool>> RecallAssignmentsAsync(Guid documentId, Guid directorId);
        Task<ApiResponse<bool>> CoordinatorAssignAsync(CoordinatorAssignRequest request, Guid coordinatorId);
    }
}