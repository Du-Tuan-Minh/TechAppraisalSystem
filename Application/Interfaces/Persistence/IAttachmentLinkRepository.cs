using Application.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Persistence
{
    public interface IAttachmentLinkRepository : IRepository<AttachmentLink>
    {
        Task<PagedResult<AttachmentLink>> GetPagedLinksByEntityIdsAsync(
               IEnumerable<Guid> entityIds,
               LinkedEntityType entityType,
               PaginationDto pagination);

        Task<IEnumerable<AttachmentLink>> GetLinksByEntityIdsAsync(
              IEnumerable<Guid> entityIds,
              LinkedEntityType entityType);
    }
}