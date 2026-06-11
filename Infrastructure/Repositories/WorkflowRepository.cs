//using Application.Interfaces.Persistence;
//using Domain.Entities;
//using Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace Infrastructure.Repositories
//{
//    public class WorkflowRepository : BaseRepository<ApprovalWorkflow>, IWorkflowRepository
//    {
//        public WorkflowRepository(ApplicationDbContext context, ILogger<WorkflowRepository> logger)
//            : base(context, logger) { }

//        public async Task<ApprovalWorkflow?> GetCurrentStepAsync(Guid documentId, Guid versionId)
//        {
//            return await _dbSet
//                .FirstOrDefaultAsync(x => x.DocumentId == documentId &&
//                                         x.RequestVersionId == versionId &&
//                                         x.IsCurrentStep);
//        }

//        public async Task<List<ApprovalWorkflow>> GetWorkflowStatusAsync(Guid documentId)
//        {
//            return await _dbSet
//                .Include(x => x.Approver)
//                  .ThenInclude(u => u.Profile)
//                .Where(x => x.DocumentId == documentId)
//                .OrderBy(x => x.StepOrder)
//                .ToListAsync();
//        }

//        public async Task<List<ApprovalWorkflow>> GetFullProcessStatusAsync(Guid documentId, Guid versionId)
//        {
//            return await _dbSet
//                .AsNoTracking()
//                .Include(x => x.Approver).ThenInclude(u => u.Profile)
//                // Lấy thông tin đợt thẩm định và danh sách người thẩm định
//                .Include(x => x.AppraisalAssignment)
//                    .ThenInclude(aa => aa.Reviewers).ThenInclude(r => r.Staff).ThenInclude(s => s.Profile)
//                .Where(x => x.DocumentId == documentId && x.RequestVersionId == versionId)
//                .OrderBy(x => x.StepOrder)
//                .ToListAsync();
//        }
//    }
//}
