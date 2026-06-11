using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class AttachmentRepository: BaseRepository<Attachment>, IAttachmentRepository
    {
        public AttachmentRepository(ApplicationDbContext context, ILogger<AttachmentRepository> logger)
            : base(context, logger)
    { }

    }
}