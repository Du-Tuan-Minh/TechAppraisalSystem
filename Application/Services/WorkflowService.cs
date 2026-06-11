namespace Application.Services
{
    //public class WorkflowService : BaseService, IWorkflowService
    //{
    //    private readonly IMapper _mapper;

    //    public WorkflowService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
    //    {
    //        _mapper = mapper;
    //    }
    //
    //public async Task<ApiResponse<SigningWorkflowResponseDto>> GetWorkflowProgressAsync(Guid documentId, Guid versionId)
    //{
    //var doc = await _unitOfWork.TechnicalDocument.GetByIdAsync(
    //    documentId,
    //    d => d.Requester.Profile!,
    //    d => d.CurrentHandler!.Profile!);
    //if (doc == null) return ApiResponse<SigningWorkflowResponseDto>.Failure(400, "Document không tồn tại.");

    //var stepsTask = _unitOfWork.ApprovalWorkflows.GetFullProcessStatusAsync(documentId, versionId);
    //var assignmentsTask = _unitOfWork.AppraisalAssignments.GetAllAsQueryable()
    //    .Where(a => a.DocumentId == documentId && a.RequestVersionId == versionId)
    //    .Include(a => a.Department)
    //    .Include(a => a.ResponsibleManager).ThenInclude(u => u.Profile)
    //    .Include(a => a.Reviewers).ThenInclude(r => r.Staff).ThenInclude(u => u.Profile)
    //    .OrderBy(a => a.CreatedAt)
    //    .ToListAsync();
    //var timelineTask = _unitOfWork.AppraisalHistory.GetAllAsQueryable()
    //    .Where(h => h.DocumentId == documentId && h.RequestVersionId == versionId)
    //    .Include(h => h.Handler).ThenInclude(u => u.Profile)
    //    .OrderBy(h => h.CreatedAt)
    //    .ToListAsync();

    //await Task.WhenAll(stepsTask, assignmentsTask, timelineTask);

    //var steps = await stepsTask;
    //var assignments = await assignmentsTask;
    //var timeline = await timelineTask;

    //var departmentAppraisals = new List<DepartmentAppraisalProgressDto>();
    //var flatReviewers = new List<ReviewerInfoDTO>();

    //foreach (var a in assignments)
    //{
    //    var deptName = a.Department?.NameDepartment;
    //    var mgrName = a.ResponsibleManager != null
    //        ? (a.ResponsibleManager.Profile != null
    //            ? a.ResponsibleManager.Profile.FullName
    //            : a.ResponsibleManager.EmployeeCode)
    //        : null;

    //    var reviewers = a.Reviewers
    //        .OrderBy(r => r.CreatedAt)
    //        .Select(r =>
    //        {
    //            var dto = _mapper.Map<ReviewerInfoDTO>(r);
    //            dto.DepartmentName = deptName;
    //            return dto;
    //        })
    //        .ToList();

    //    flatReviewers.AddRange(reviewers);

    //    departmentAppraisals.Add(new DepartmentAppraisalProgressDto
    //    {
    //        Id = a.Id,
    //        DepartmentId = a.DepartmentId,
    //        DepartmentName = deptName,
    //        ResponsibleManagerName = mgrName,
    //        Status = a.Status,
    //        Deadline = a.Deadline,
    //        CompletedAt = a.CompletedAt,
    //        ManagerComment = a.ManagerComment,
    //        Reviewers = reviewers
    //    });
    //}

    //var result = new SigningWorkflowResponseDto
    //{
    //    DocumentId = documentId,
    //    VersionId = versionId,
    //    Title = doc.Title ?? "",
    //    CurrentStatus = doc.Status,
    //    CreatorName = doc.Requester?.Profile?.FullName ?? doc.Requester?.EmployeeCode,
    //    CreatedAt = doc.CreatedAt,
    //    CurrentHandlerId = doc.CurrentHandlerId,
    //    CurrentHandlerName = doc.CurrentHandler != null
    //        ? (doc.CurrentHandler.Profile?.FullName ?? doc.CurrentHandler.EmployeeCode)
    //        : null,

    //    WorkflowSteps = _mapper.Map<List<ApprovalStepResponseDto>>(steps),

    //    Reviewers = flatReviewers,
    //    DepartmentAppraisals = departmentAppraisals,
    //    VersionTimeline = _mapper.Map<List<WorkflowVersionEventDto>>(timeline)
    //};

    //return ApiResponse<SigningWorkflowResponseDto>.Success("result");

    //    }
    //}
}
