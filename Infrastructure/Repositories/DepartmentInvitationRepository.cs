using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class DepartmentInvitationRepository : BaseRepository<DepartmentInvitation>, IDepartmentInvitationRepository
    {
        public DepartmentInvitationRepository(ApplicationDbContext context, ILogger<DepartmentInvitationRepository> logger)
            : base(context, logger) { }

        public async Task<DepartmentInvitation?> GetValidInvitationAsync(string code, string EmployeeCode)
        {
            var results = await FindAsync(i =>
                i.InvitationCode == code &&
                i.InviteeEmployeeCode == EmployeeCode &&
                !i.IsUsed &&
                i.ExpiresAt > DateTime.UtcNow);
            return results.FirstOrDefault();
        }

        public async Task<bool> HasActiveInvitationAsync(string EmployeeCode, Guid departmentId)
        {
            return await AnyAsync(i =>
                i.InviteeEmployeeCode == EmployeeCode &&
                i.DepartmentId == departmentId &&
                !i.IsUsed &&
                i.ExpiresAt > DateTime.UtcNow);
        }
    }
}
