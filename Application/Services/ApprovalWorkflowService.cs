using Application.Common;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;

namespace Application.Services
{
    public class ApprovalWorkflowService : BaseService, IApprovalWorkflowService
    {
        private readonly IMapper _mapper;

        public ApprovalWorkflowService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ApprovalWorkflowResponseDto>>> GetDocumentWorkflowAsync(Guid documentId, Guid? requestVersionId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var documentExists = await _unitOfWork.TechnicalDocument.AnyAsync(x => x.Id == documentId);
                if (!documentExists) return ApiResponse<List<ApprovalWorkflowResponseDto>>.Failure(404, "Document does not exist.");
                var workflows = await _unitOfWork.ApprovalWorkflows.GetDocumentWorkflowAsync(documentId, requestVersionId);

                return ApiResponse<List<ApprovalWorkflowResponseDto>>.Success(_mapper.Map<List<ApprovalWorkflowResponseDto>>(workflows));
            });
        }

        public async Task<ApiResponse<ApprovalWorkflowDetailDto>> GetWorkflowDetailAsync(Guid id)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var workflow = await _unitOfWork.ApprovalWorkflows
                    .GetByIdAsync(
                        id,
                        x => x.Approver!,
                        x => x.Approver!.Profile!);

                if (workflow == null) return ApiResponse<ApprovalWorkflowDetailDto>.Failure(404, "Workflow does not exist.");

                return ApiResponse<ApprovalWorkflowDetailDto>.Success(_mapper.Map<ApprovalWorkflowDetailDto>(workflow));
            });
        }
    }
}