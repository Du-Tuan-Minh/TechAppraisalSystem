using Application.Common;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class TechnicalDocumentRepository : BaseRepository<TechnicalDocument>, ITechnicalDocumentRepository
    {
        public TechnicalDocumentRepository(ApplicationDbContext context, ILogger<TechnicalDocumentRepository> logger)
            : base(context, logger)
        { }

        public async Task<TechnicalDocument?> GetDetailDocumentAsync(Guid id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(d => d.Requester).ThenInclude(u => u.Profile)
                .Include(d => d.CurrentHandler).ThenInclude(u => u.Profile)
                .Include(d => d.Versions.OrderByDescending(v => v.VersionNumber))
                // .Include(d => d.Attachments)
                //.Include(d => d.AppraisalHistories.OrderByDescending(h => h.CreatedAt))
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> IsOwnerAsync(Guid documentId, Guid userId)
        {
            return await _dbSet.AnyAsync(d => d.Id == documentId && d.RequesterId == userId);
        }

        public async Task<PagedResult<TechnicalDocument>> GetDocumentsFilteredAsync(DocumentFilterDto filter, Guid userId, UserRole role, Guid? userDepartmentId)
        {
            var query = _dbSet.AsNoTracking()
                              .Include(d => d.Requester).ThenInclude(u => u.Profile)
                              .Include(d => d.Department)
                              .AsQueryable();

            if (role == UserRole.Staff || role == UserRole.Manager)
                query = query.Where(d => d.DepartmentId == userDepartmentId);
            else if (role == UserRole.Inspector)
                query = query.Where(d => d.Status == DocumentStatus.Issued || d.Status == DocumentStatus.Signing);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(d => d.Title.ToLower().Contains(term) || d.DocumentCode.ToLower().Contains(term));
            }

            if (filter.Type.HasValue) query = query.Where(d => d.Type == filter.Type.Value);
            if (filter.Status.HasValue) query = query.Where(d => d.Status == filter.Status.Value);
            if (filter.FromDate.HasValue) query = query.Where(d => d.CreatedAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue) query = query.Where(d => d.CreatedAt <= filter.ToDate.Value);

            return await query.OrderByDescending(d => d.CreatedAt).ToPagedListAsync(filter.Page, filter.PageSize);
        }

        public async Task<PagedResult<TechnicalDocument>> GetMyTasksAsync(Guid userId, PaginationDto pagination)
        {
            var query = _dbSet.AsNoTracking()
                              .Include(d => d.Requester).ThenInclude(u => u.Profile)
                              .Include(d => d.Department)
                              .Include(d => d.Versions)
                              .Include(d => d.AppraisalAssignments)
                                  .ThenInclude(a => a.Version)
                              .Include(d => d.AppraisalAssignments)
                                  .ThenInclude(a => a.Reviewers)
                              .AsQueryable();
            query = query.Where(d =>
                d.CurrentHandlerId == userId ||
                (
                    d.Status != DocumentStatus.Issued &&
                    d.Status != DocumentStatus.Archived &&
                    d.Status != DocumentStatus.Rejected &&
                    //   d.Status != DocumentStatus.Signing &&

                    d.AppraisalAssignments.Any(a =>
                        (a.ResponsibleManagerId == userId ||
                         a.Reviewers.Any(r => r.StaffId == userId))
                        &&
                        (a.Status == AssignmentStatus.Pending ||
                         a.Status == AssignmentStatus.InReview ||
                         a.Status == AssignmentStatus.Overdue ||
                         a.Status == AssignmentStatus.AwaitingClarification)
                    )
                )
            );

            return await query.OrderByDescending(d => d.Priority)
                              .ThenByDescending(d => d.CreatedAt)
                              .ToPagedListAsync(pagination.Page, pagination.PageSize);
        }

        public async Task<TechnicalDocument?> GetVersionWithReviewersAsync(Guid versionId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(d => d.Requester)
                .Include(d => d.Versions.Where(v => v.Id == versionId))
                    .ThenInclude(v => v.AppraisalAssignments)
                        .ThenInclude(a => a.Reviewers)
                .Where(d => d.Versions.Any(v => v.Id == versionId))
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<TechnicalDocument>> GetMyCurrentDocumentsAsync(Guid userId, UserCurrentDocumentFilterDto filter)
        {
            var query = _dbSet.AsNoTracking()
                .Include(d => d.Versions)
                .Include(d => d.CurrentHandler)
                    .ThenInclude(u => u.Profile)
                .Include(d => d.AppraisalAssignments)
                    .ThenInclude(a => a.Version)
                .Where(d =>
                    d.RequesterId == userId &&
                    d.Versions.Any(v => v.IsCurrent));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();

                query = query.Where(d =>
                    d.Title.ToLower().Contains(term) ||
                    d.DocumentCode.ToLower().Contains(term));
            }

            if (filter.Priority.HasValue)
                query = query.Where(d => d.Priority == filter.Priority.Value);

            switch (filter.Type)
            {
                case StaffDashboardDocumentType.Draft:
                    query = query.Where(x =>
                        x.Status == DocumentStatus.Draft);
                    break;

                case StaffDashboardDocumentType.ReturnedForRevision:
                    query = query.Where(x =>
                        x.Status == DocumentStatus.AdjustmentRequired ||
                        x.Status == DocumentStatus.Rejected);
                    break;

                case StaffDashboardDocumentType.InternalReview:
                    query = query.Where(x =>
                        x.Status == DocumentStatus.InternalPending ||
                        x.Status == DocumentStatus.InternalApproved);
                    break;

                case StaffDashboardDocumentType.Appraisal:
                    query = query.Where(x =>
                        x.Status == DocumentStatus.AppraisalPending ||
                        x.Status == DocumentStatus.Appraising);
                    break;

                case StaffDashboardDocumentType.Signing:
                    query = query.Where(x =>
                        x.Status == DocumentStatus.Signing);
                    break;

                case StaffDashboardDocumentType.Issued:
                    query = query.Where(x =>
                        x.Status == DocumentStatus.Issued);
                    break;

                case StaffDashboardDocumentType.Overdue:
                    query = query.Where(x =>
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
                        ));
                    break;
            }

            return await query
                .OrderByDescending(d => d.Priority)
                .ThenByDescending(d => d.CreatedAt)
                .ToPagedListAsync(filter.Page, filter.PageSize);
        }

        public async Task<PagedResult<TechnicalDocument>> GetManagerStatusDocumentsAsync(Guid managerId, ManagerDashboardDocumentFilterDto filter)
        {
            var manager = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == managerId);

            if (manager?.DepartmentId == null) return new PagedResult<TechnicalDocument>();

            var departmentId = manager.DepartmentId.Value;

            IQueryable<TechnicalDocument> query = _context.TechnicalDocuments
                .AsNoTracking()
                .Include(d => d.CurrentHandler)
                    .ThenInclude(u => u.Profile)
                .Include(d => d.AppraisalAssignments
                    .Where(a => a.DepartmentId == departmentId))
                    .ThenInclude(a => a.Reviewers)
                        .ThenInclude(r => r.Staff)
                            .ThenInclude(s => s.Profile);

            switch (filter.Type)
            {
                case ManagerDashboardDocumentType.Review:
                    query = query.Where(d =>
                        (
                            d.DepartmentId == departmentId &&
                            (
                                d.Status == DocumentStatus.InternalPending ||
                                d.Status == DocumentStatus.InternalApproved
                            )
                        )
                        ||
                        d.AppraisalAssignments.Any(a =>
                            a.DepartmentId == departmentId &&
                            (
                                a.Status == AssignmentStatus.Pending ||
                                a.Status == AssignmentStatus.InReview
                            )));
                    break;

                case ManagerDashboardDocumentType.NeedConfirmation:
                    query = query.Where(d =>
                        d.AppraisalAssignments.Any(a =>
                            a.DepartmentId == departmentId &&
                            a.Status == AssignmentStatus.Completed));
                    break;

                case ManagerDashboardDocumentType.Rejected:
                    query = query.Where(d =>
                        d.DepartmentId == departmentId &&
                        (
                            d.Status == DocumentStatus.Rejected ||
                            d.Status == DocumentStatus.AdjustmentRequired
                        ));
                    break;
                case ManagerDashboardDocumentType.Overdue:
                    query = query.Where(d =>
                        d.AppraisalAssignments.Any(a =>
                            a.DepartmentId == departmentId &&
                            a.Reviewers.Any(r =>
                                r.Deadline != null &&
                                r.Deadline < DateTime.UtcNow &&
                                (
                                    r.Status == ReviewerStatus.Pending ||
                                    r.Status == ReviewerStatus.Reviewing
                                ))));
                    break;
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();

                query = query.Where(d =>
                    EF.Functions.ILike(d.Title, $"%{term}%") ||
                    EF.Functions.ILike(d.DocumentCode, $"%{term}%"));
            }

            return await query.Distinct()
                .OrderByDescending(d => d.Priority)
                .ThenByDescending(d => d.CreatedAt)
                .ToPagedListAsync(filter.Page, filter.PageSize);
        }

        //public async Task<PagedResult<AppraisalReviewer>> GetPendingAppraisalResponsesAsync(Guid managerId, PendingAppraisalFilterDto filter)
        //{
        //    var manager = await _context.Users
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(x => x.Id == managerId);

        //    if (manager?.DepartmentId == null)
        //        return new PagedResult<AppraisalReviewer>();

        //    var query = _context.AppraisalReviewers
        //        .AsNoTracking()
        //        .Include(x => x.Staff)
        //            .ThenInclude(x => x.Profile)
        //        .Include(x => x.Assignment)
        //            .ThenInclude(x => x.Document)
        //        .Where(x =>
        //            x.Assignment.DepartmentId == manager.DepartmentId &&
        //            (x.Status == ReviewerStatus.Pending ||
        //             x.Status == ReviewerStatus.Reviewing));

        //    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        //    {
        //        var term = filter.SearchTerm.Trim().ToLower();

        //        query = query.Where(x =>
        //            x.Assignment.Document.Title.ToLower().Contains(term) ||
        //            x.Assignment.Document.DocumentCode.ToLower().Contains(term) ||
        //            x.Staff.EmployeeCode.ToLower().Contains(term));
        //    }

        //    if (filter.Status.HasValue)
        //    {
        //        query = query.Where(x => x.Status == filter.Status.Value);
        //    }

        //    return await query
        //        .OrderBy(x => x.Deadline)
        //        .ToPagedListAsync(filter.Page, filter.PageSize);
        //}

        public async Task<PagedResult<AppraisalReviewer>> GetOverdueDocumentsAsync(Guid managerId, OverdueFilterDto filter)
        {
            var manager = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == managerId);

            if (manager?.DepartmentId == null) return new PagedResult<AppraisalReviewer>();

            var query = _context.AppraisalReviewers
                .AsNoTracking()
                .Include(x => x.Staff)
                    .ThenInclude(x => x.Profile)
                .Include(x => x.Assignment)
                    .ThenInclude(x => x.Document)
                        .ThenInclude(x => x.Requester)
                            .ThenInclude(x => x.Profile)
                .Where(x =>
                    x.Assignment.DepartmentId == manager.DepartmentId &&
                    x.Deadline != null &&
                    x.Deadline < DateTime.UtcNow &&
                    (x.Status == ReviewerStatus.Pending ||
                     x.Status == ReviewerStatus.Reviewing));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();

                query = query.Where(x =>
                    x.Assignment.Document.Title.ToLower().Contains(term) ||
                    x.Assignment.Document.DocumentCode.ToLower().Contains(term) ||
                    x.Staff.EmployeeCode.ToLower().Contains(term));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            return await query
                .OrderBy(x => x.Deadline)
                .ToPagedListAsync(filter.Page, filter.PageSize);
        }

        public async Task<PagedResult<AppraisalReviewer>> GetManagerRequestOverdueDocumentsAsync(Guid directorId, OverdueFilterDto filter)
        {
            var director = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == directorId);

            if (director?.DepartmentId == null) return new PagedResult<AppraisalReviewer>();

            var departmentId = director.DepartmentId.Value;

            var query = _context.AppraisalReviewers
                .AsNoTracking()
                .Include(x => x.Staff)
                    .ThenInclude(x => x.Profile)
                .Include(x => x.Assignment)
                    .ThenInclude(x => x.Document)
                        .ThenInclude(x => x.Requester)
                            .ThenInclude(x => x.Profile)
                .Where(x =>
                    // tài liệu do phòng ban mình tạo
                    x.Assignment.Document.DepartmentId == departmentId &&

                    // đang được phòng ban khác review
                    x.Assignment.DepartmentId != departmentId &&

                    // quá hạn
                    x.Deadline != null &&
                    x.Deadline < DateTime.UtcNow &&

                    (
                        x.Status == ReviewerStatus.Pending ||
                        x.Status == ReviewerStatus.Reviewing
                    ));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();

                query = query.Where(x =>
                    EF.Functions.ILike(x.Assignment.Document.Title, $"%{term}%") ||
                    EF.Functions.ILike(x.Assignment.Document.DocumentCode, $"%{term}%") ||
                    EF.Functions.ILike(x.Staff.EmployeeCode, $"%{term}%"));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            return await query
                .OrderBy(x => x.Deadline)
                .ToPagedListAsync(filter.Page, filter.PageSize);
        }

        public async Task<PagedResult<TechnicalDocument>> GetManagerRequestStatusDocumentsAsync(Guid managerId, DepartmentDocumentStatusFilterDto filter)
        {
            var manager = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == managerId);

            if (manager?.DepartmentId == null) return new PagedResult<TechnicalDocument>();

            var departmentId = manager.DepartmentId.Value;

            IQueryable<TechnicalDocument> query = _context.TechnicalDocuments
                .AsNoTracking()
                .Include(d => d.CurrentHandler)
                    .ThenInclude(u => u.Profile)
                .Include(d => d.Requester)
                    .ThenInclude(u => u.Profile)
                .Include(d => d.AppraisalAssignments)
                    .ThenInclude(a => a.Reviewers)
                        .ThenInclude(r => r.Staff)
                            .ThenInclude(s => s.Profile)
                .Where(d => d.DepartmentId == departmentId);

            switch (filter.Type)
            {
                case DepartmentDocumentStatusType.Reviewing:
                    query = query.Where(d =>
                    d.Status == DocumentStatus.AppraisalPending || d.Status == DocumentStatus.Appraising &&
                        d.AppraisalAssignments.Any(a =>
                            a.DepartmentId != departmentId &&
                            (
                                a.Status == AssignmentStatus.Pending ||
                                a.Status == AssignmentStatus.InReview
                            )));
                    break;

                case DepartmentDocumentStatusType.OverdueReview:
                    query = query.Where(d =>
                        d.AppraisalAssignments.Any(a =>
                        (d.Status == DocumentStatus.AppraisalPending || d.Status == DocumentStatus.Appraising) &&
                            a.DepartmentId != departmentId &&
                            a.Deadline != null &&
                            a.Deadline < DateTime.UtcNow &&
                            (
                                a.Status == AssignmentStatus.Pending ||
                                a.Status == AssignmentStatus.InReview
                            )));
                    break;

                case DepartmentDocumentStatusType.Issued:
                    query = query.Where(d =>
                        d.Status == DocumentStatus.Issued);
                    break;

                case DepartmentDocumentStatusType.Rejected:
                    query = query.Where(d =>
                        d.Status == DocumentStatus.Rejected ||
                        d.Status == DocumentStatus.AdjustmentRequired);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();

                query = query.Where(d =>
                    EF.Functions.ILike(d.Title, $"%{term}%") ||
                    EF.Functions.ILike(d.DocumentCode, $"%{term}%"));
            }

            return await query
                .Distinct()
                .OrderByDescending(d => d.Priority)
                .ThenByDescending(d => d.CreatedAt)
                .ToPagedListAsync(filter.Page, filter.PageSize);
        }

        public async Task<PagedResult<TechnicalDocument>> GetIncomingAppraisalDocumentsAsync(Guid departmentId, PaginationDto pagination, string? searchTerm)
        {
            var query = _context.AppraisalAssignments
                   .AsNoTracking()
                   .Where(a =>
                       a.DepartmentId == departmentId &&
                       (a.Status == AssignmentStatus.Pending ||
                        a.Status == AssignmentStatus.InReview) &&
                       (a.Document.Status == DocumentStatus.AppraisalPending ||
                        a.Document.Status == DocumentStatus.Appraising))
                   .Select(a => a.Document)
                   .Distinct();

            query = query.Include(x => x.Versions);
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(d =>
                    EF.Functions.ILike(d.Title, $"%{searchTerm}%") ||
                    EF.Functions.ILike(d.DocumentCode, $"%{searchTerm}%"));
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToPagedListAsync(
                    pagination.Page,
                    pagination.PageSize);
        }

        public async Task<PagedResult<TechnicalDocument>> GetUserWorkloadDocumentsAsync(Guid userId, PaginationDto pagination, string? searchTerm)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null) return new PagedResult<TechnicalDocument>();

            IQueryable<TechnicalDocument> query;

            if (user.Role == UserRole.Manager)
            {
                query = _context.AppraisalAssignments
                    .AsNoTracking()
                    .Where(a =>
                           a.ResponsibleManagerId == userId &&
                           (a.Status == AssignmentStatus.Pending ||
                            a.Status == AssignmentStatus.InReview) &&
                           (
                               a.Document.Status == DocumentStatus.AppraisalPending ||
                               a.Document.Status == DocumentStatus.Appraising
                           ))
                    .Select(a => a.Document)
                    .Distinct();
            }
            else if (user.Role == UserRole.Staff)
            {
                query = _context.AppraisalReviewers
                    .AsNoTracking()
                   .Where(r =>
                          r.StaffId == userId &&
                          (r.Status == ReviewerStatus.Pending ||
                           r.Status == ReviewerStatus.Reviewing) &&
                          (
                              r.Assignment.Document.Status == DocumentStatus.AppraisalPending ||
                              r.Assignment.Document.Status == DocumentStatus.Appraising
                          ))
                    .Select(r => r.Assignment.Document)
                    .Distinct();
            }
            else
            {
                query = Enumerable.Empty<TechnicalDocument>().AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();

                query = query.Where(x =>
                    EF.Functions.ILike(x.DocumentCode, $"%{term}%") ||
                    EF.Functions.ILike(x.Title, $"%{term}%"));
            }

            query = query.Include(x => x.Versions);
            query = query.OrderByDescending(x => x.CreatedAt);

            return await query.ToPagedListAsync(
                pagination.Page,
                pagination.PageSize);
        }
    }
}