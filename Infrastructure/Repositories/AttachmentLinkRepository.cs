using Application.Common;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class AttachmentLinkRepository : BaseRepository<AttachmentLink>, IAttachmentLinkRepository
    {
        public AttachmentLinkRepository(ApplicationDbContext context, ILogger<AttachmentLinkRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<PagedResult<AttachmentLink>> GetPagedLinksByEntityIdsAsync(
              IEnumerable<Guid> entityIds,
              LinkedEntityType entityType,
              PaginationDto pagination)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(al => al.Attachment)
                .Where(al => entityIds.Contains(al.EntityId) && al.EntityType == entityType)
                .OrderByDescending(al => al.CreatedAt);

            return await query.ToPagedListAsync(pagination.Page, pagination.PageSize);
        }

        public async Task<IEnumerable<AttachmentLink>> GetLinksByEntityIdsAsync(
              IEnumerable<Guid> entityIds,
              LinkedEntityType entityType)
        {
            if (entityIds == null || !entityIds.Any()) return new List<AttachmentLink>();

            return await _dbSet
                .AsNoTracking()
                .Include(al => al.Attachment)
                .Where(al => entityIds.Contains(al.EntityId) && al.EntityType == entityType)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();
        }
    }
}