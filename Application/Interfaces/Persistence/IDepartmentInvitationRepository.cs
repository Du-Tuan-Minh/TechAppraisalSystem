using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDepartmentInvitationRepository : IRepository<DepartmentInvitation>
    {
        Task<bool> HasActiveInvitationAsync(string EmployeeCode, Guid departmentId);
        Task<DepartmentInvitation?> GetValidInvitationAsync(string code, string EmployeeCode);
    }
}
