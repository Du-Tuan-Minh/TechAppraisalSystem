using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class ApprovalWorkflowRepository : BaseRepository<ApprovalWorkflow>, IApprovalWorkflowRepository
    {
        public ApprovalWorkflowRepository(ApplicationDbContext context, ILogger<ApprovalWorkflowRepository> logger)
            : base(context, logger) { }

        public async Task<List<ApprovalWorkflow>> GetDocumentWorkflowAsync(Guid documentId, Guid? requestVersionId)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(x => x.Approver)
                    .ThenInclude(x => x!.Profile)
                .Where(x => x.DocumentId == documentId);

            if (requestVersionId.HasValue)
            {
                query = query.Where(x => x.RequestVersionId == requestVersionId.Value);
            }
            else
            {
                query = query.Where(x => x.Version.IsCurrent);
            }

            return await query.OrderBy(x => x.StepOrder).ToListAsync();
        }
    }
}