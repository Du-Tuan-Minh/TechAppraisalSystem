using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IApprovalWorkflowRepository : IRepository<ApprovalWorkflow>
    {
        Task<List<ApprovalWorkflow>> GetDocumentWorkflowAsync(Guid documentId, Guid? requestVersionId);
    }
}