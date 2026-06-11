using Application.Common;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryStaffDto> GetStaffDashboardSummaryAsync(Guid staffId)
        {
            var query = _context.Set<TechnicalDocument>().AsNoTracking()
                .Where(x => x.RequesterId == staffId);

            return new DashboardSummaryStaffDto
            {
                DraftCount = await query.CountAsync(x => x.Status == DocumentStatus.Draft),

                ReturnedForRevisionCount = await query.CountAsync(x =>
                    x.Status == DocumentStatus.AdjustmentRequired ||
                    x.Status == DocumentStatus.Rejected),

                InternalReviewCount = await query.CountAsync(x =>
                    x.Status == DocumentStatus.InternalPending ||
                    x.Status == DocumentStatus.InternalApproved),

                AppraisalCount = await query.CountAsync(x =>
                    x.Status == DocumentStatus.AppraisalPending ||
                    x.Status == DocumentStatus.Appraising),

                SigningCount = await query.CountAsync(x => x.Status == DocumentStatus.Signing),

                IssuedCount = await query.CountAsync(x => x.Status == DocumentStatus.Issued),

                OverdueCount = await query.CountAsync(x =>
                     (
                         x.Status == DocumentStatus.AppraisalPending ||
                         x.Status == DocumentStatus.Appraising
                     ) &&
                    x.AppraisalAssignments.Any(a =>

                        (
                            a.Deadline != null &&
                            a.Deadline < DateTime.UtcNow &&
                            (
                                a.Status == AssignmentStatus.Pending ||
                                a.Status == AssignmentStatus.InReview
                            )
                        )

                        ||

                        a.Reviewers.Any(r =>
                            r.Deadline != null &&
                            r.Deadline < DateTime.UtcNow &&
                            (
                                r.Status == ReviewerStatus.Pending ||
                                r.Status == ReviewerStatus.Reviewing
                             ))
            ))
            };
        }

        public async Task<DashboardSummaryManagerDto> GetManagerDashboardSummaryAsync(Guid managerId)
        {
            var manager = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == managerId);

            if (manager == null || manager.DepartmentId == null) return new DashboardSummaryManagerDto();

            var departmentId = manager.DepartmentId.Value;

            var documents = _context.TechnicalDocuments.AsNoTracking()
                .Where(x => x.DepartmentId == departmentId);

            var assignments = _context.AppraisalAssignments.AsNoTracking()
                .Where(x => x.DepartmentId == departmentId);

            var reviewers = _context.AppraisalReviewers.AsNoTracking();

            return new DashboardSummaryManagerDto
            {
                ReviewingDocuments =
                (await documents.CountAsync(x =>
                    x.Status == DocumentStatus.InternalPending ||
                    x.Status == DocumentStatus.InternalApproved))
                +
                (await assignments.CountAsync(x =>
                    x.Status == AssignmentStatus.Pending ||
                    x.Status == AssignmentStatus.InReview)),

                OverdueReviewDocuments = await reviewers.CountAsync(x =>
                    x.Assignment.DepartmentId == departmentId &&
                    x.Deadline != null &&
                    x.Deadline < DateTime.UtcNow &&
                    (x.Status == ReviewerStatus.Pending ||
                     x.Status == ReviewerStatus.Reviewing)),

                NeedConfirmationCount = await assignments.CountAsync(x =>
                    x.Status == AssignmentStatus.Completed),

                RejectedCount = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.Rejected ||
                    x.Status == DocumentStatus.AdjustmentRequired)
            };
        }

        public async Task<DepartmentDocumentStatusSummaryDto> GetDepartmentDocumentStatusSummaryAsync(Guid managerId)
        {
            var manager = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == managerId);

            if (manager?.DepartmentId == null) return new DepartmentDocumentStatusSummaryDto();

            var departmentId = manager.DepartmentId.Value;

            var documents = _context.TechnicalDocuments
                .AsNoTracking()
                .Where(x => x.DepartmentId == departmentId);

            return new DepartmentDocumentStatusSummaryDto
            {
                ReviewingDocuments = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.AppraisalPending || x.Status == DocumentStatus.Appraising &&
                    x.AppraisalAssignments.Any(a =>
                        a.DepartmentId != departmentId &&
                        (
                            a.Status == AssignmentStatus.Pending ||
                            a.Status == AssignmentStatus.InReview
                        ))),

                OverdueReviewDocuments = await documents.CountAsync(x =>
                   (x.Status == DocumentStatus.AppraisalPending || x.Status == DocumentStatus.Appraising) &&
                    x.AppraisalAssignments.Any(a =>
                        a.DepartmentId != departmentId &&
                        a.Deadline != null &&
                        a.Deadline < DateTime.UtcNow &&
                        (
                            a.Status == AssignmentStatus.Pending ||
                            a.Status == AssignmentStatus.InReview
                        ))),

                IssuedDocuments = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.Issued),

                RejectedDocuments = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.Rejected ||
                    x.Status == DocumentStatus.AdjustmentRequired)
            };
        }

        public async Task<DashboardSummaryDirectorDto> GetDirectorDashboardSummaryAsync(Guid directorId)
        {
            var director = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == directorId);

            if (director?.DepartmentId == null) return new DashboardSummaryDirectorDto();

            var departmentId = director.DepartmentId.Value;

            var childDepartmentIds = await _context.Departments
                .AsNoTracking()
                .Where(x => x.ParentId == departmentId)
                .Select(x => x.Id)
                .ToListAsync();

            childDepartmentIds.Add(departmentId);

            var documents = _context.TechnicalDocuments
                .AsNoTracking()
                .Where(x => childDepartmentIds.Contains(x.DepartmentId));

            return new DashboardSummaryDirectorDto
            {
                DraftingCount = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.Draft),

                AppraisingCount = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.InternalPending ||
                    x.Status == DocumentStatus.InternalApproved ||
                    x.Status == DocumentStatus.AppraisalPending ||
                    x.Status == DocumentStatus.Appraising),

                SigningCount = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.Signing),

                IssuedCount = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.Issued),

                RejectedCount = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.Rejected ||
                    x.Status == DocumentStatus.AdjustmentRequired),

                OverdueSigningCount = await documents.CountAsync(x =>
                    x.Status == DocumentStatus.Signing &&
                    x.UpdatedAt != null &&
                    x.UpdatedAt < DateTime.UtcNow.AddDays(-3))
            };
        }

        public async Task<PagedResult<ManagerWorkloadDto>> GetManagerWorkloadsAsync(Guid directorId, PaginationDto pagination, string? searchTerm)
        {
            var director = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == directorId);

            if (director?.DepartmentId == null) return new PagedResult<ManagerWorkloadDto>();

            var departmentId = director.DepartmentId.Value;

            var childDepartments = await _context.Departments
                .AsNoTracking()
                .Where(x => x.ParentId == departmentId)
                .ToListAsync();

            var childDepartmentIds = childDepartments
                .Select(x => x.Id)
                .ToList();

            var managers = _context.Users
                .AsNoTracking()
                .Where(x =>
                    x.Role == UserRole.Manager &&
                    x.DepartmentId != null &&
                    childDepartmentIds.Contains(x.DepartmentId.Value));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = $"%{searchTerm.Trim()}%";

                managers = managers.Where(x =>
                    EF.Functions.Like(x.EmployeeCode, term) ||
                    EF.Functions.Like(x.Profile!.FirstName!, term) ||
                    EF.Functions.Like(x.Profile!.LastName!, term) ||
                    EF.Functions.Like(x.Department!.NameDepartment, term));
            }

            var dtoQuery = managers
                .Select(x => new ManagerWorkloadDto
                {
                    ManagerId = x.Id,

                    ManagerName = x.Profile != null
                        ? x.Profile.FullName
                        : x.EmployeeCode,

                    DepartmentName = x.Department != null
                        ? x.Department.NameDepartment
                        : string.Empty,

                    PendingAssignments = x.ManagedAssignments.Count(a =>
                        a.Status == AssignmentStatus.Pending),

                    InReviewAssignments = x.ManagedAssignments.Count(a =>
                        a.Status == AssignmentStatus.InReview),

                    OverdueAssignments = x.ManagedAssignments.Count(a =>
                        a.Deadline != null &&
                        a.Deadline < DateTime.UtcNow &&
                        (
                            a.Status == AssignmentStatus.Pending ||
                            a.Status == AssignmentStatus.InReview
                        ))
                })
                .OrderByDescending(x =>
                    x.PendingAssignments +
                    x.InReviewAssignments +
                    x.OverdueAssignments);

            return await dtoQuery.ToPagedListAsync(
                pagination.Page,
                pagination.PageSize);
        }

        public async Task<DashboardSummaryCoordinatorDto> GetCoordinatorDashboardSummaryAsync(Guid coordinatorId)
        {
            var coordinator = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == coordinatorId);

            if (coordinator?.DepartmentId == null) return new DashboardSummaryCoordinatorDto();

            var centerDepartmentId = coordinator.DepartmentId.Value;

            var totalDepartments = await _context.Departments
                .CountAsync(x => x.ParentId == centerDepartmentId);

            var appraisalDocuments = await _context.AppraisalAssignments
                .Where(a =>
                    a.DepartmentId == centerDepartmentId &&
                    (a.Status == AssignmentStatus.Pending ||
                     a.Status == AssignmentStatus.InReview) &&
                    (a.Document.Status == DocumentStatus.AppraisalPending ||
                     a.Document.Status == DocumentStatus.Appraising))
                .Select(a => a.DocumentId)
                .Distinct()
                .CountAsync();

            return new DashboardSummaryCoordinatorDto
            {
                TotalDepartments = totalDepartments,
                PendingAssignments = appraisalDocuments
            };
        }
    }
}