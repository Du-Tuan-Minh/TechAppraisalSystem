using Application.Common;
using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class DepartmentService : BaseService, IDepartmentService
    {
        private readonly IMapper _mapper;

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public async Task<ApiResponse<string>> InviteToDepartmentAsync(Guid inviterId, DepartmentInvitationCreateDto invite)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<string>>(async () =>
            {
                var inviter = await _unitOfWork.Users.GetByIdAsync(inviterId);
                if (inviter == null) return ApiResponse<string>.Failure(404, "Người gửi lời mời không tồn tại.");
                if (inviter.Role != UserRole.Manager && inviter.Role != UserRole.Director && inviter.Role != UserRole.InstituteDirector)
                    return ApiResponse<string>.Failure(403, "Bạn không có quyền gửi lời mời.");
                if (!inviter.DepartmentId.HasValue) return ApiResponse<string>.Failure(400, "Bạn chưa thuộc đơn vị nào nên không thể gửi lời mời.");

                var targetDepartmentId = inviter.DepartmentId.Value;

                var invitee = (await _unitOfWork.Users.FindAsync(u => u.EmployeeCode == invite.EmployeeCode)).FirstOrDefault();
                if (invitee?.DepartmentId == targetDepartmentId) return ApiResponse<string>.Failure(400, "Nhân viên này đã có mặt trong đơn vị.");

                var hasActive = await _unitOfWork.DepartmentInvitations.HasActiveInvitationAsync(invite.EmployeeCode, targetDepartmentId);
                if (hasActive) return ApiResponse<string>.Failure(400, "Đã có lời mời đang chờ xử lý cho nhân viên này.");

                var code = new Random().Next(100000, 999999).ToString();
                var invitation = new DepartmentInvitation
                {
                    Id = Guid.NewGuid(),
                    DepartmentId = targetDepartmentId,
                    InviteeEmployeeCode = invite.EmployeeCode,
                    InvitationCode = code,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    IsUsed = false
                };

                await _unitOfWork.DepartmentInvitations.AddAsync(invitation);

                if (invitee != null)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Lời mời gia nhập đơn vị",
                        Content = $"{(inviter.Role == UserRole.Director ? "Giám đốc" : "Trưởng phòng")} {inviter.Profile?.FullName ?? inviter.EmployeeCode} mời bạn tham gia đơn vị. Mã xác nhận: {code}",
                        Type = NotificationType.System,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Notifications.AddAsync(notification);
                    await _unitOfWork.UserNotifications.AddAsync(new UserNotification { UserId = invitee.Id, Notification = notification });
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<string>.Success(code, "Mã mời đã được tạo (Hiệu lực 24h).");
            });
        }

        public async Task<ApiResponse<bool>> JoinDepartmentAsync(Guid userId, string inviteCode)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null) return ApiResponse<bool>.Failure(404, "Người dùng không tồn tại.");

                var invite = await _unitOfWork.DepartmentInvitations.GetValidInvitationAsync(inviteCode, user.EmployeeCode);
                if (invite == null) return ApiResponse<bool>.Failure(400, "Mã mời không chính xác, đã được sử dụng hoặc đã hết hạn.");

                var department = await _unitOfWork.Departments.GetByIdAsync(invite.DepartmentId);
                if (department == null) return ApiResponse<bool>.Failure(404, "Đơn vị mời không còn tồn tại trên hệ thống.");

                if (user.Role == UserRole.Manager && department.ParentId != null)
                {
                    return ApiResponse<bool>.Failure(400, "Trưởng phòng (Manager) chỉ được phép gia nhập cấp Trung tâm.");
                }

                user.DepartmentId = invite.DepartmentId;
                invite.IsUsed = true;

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, $"Chúc mừng! Bạn đã gia nhập đơn vị: {department.NameDepartment}");
            });
        }

        public async Task<ApiResponse<DepartmentResponseDto>> CreateDepartmentAsync(DepartmentCreateDto dto, Guid userId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<DepartmentResponseDto>>(async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null) return ApiResponse<DepartmentResponseDto>.Failure(404, "Người dùng không tồn tại.");

                var isDuplicate = await _unitOfWork.Departments.AnyAsync(d =>
                    d.NameDepartment.ToLower() == dto.NameDepartment.ToLower() ||
                    d.CodeDepartment.ToLower() == dto.CodeDepartment.ToLower());

                if (isDuplicate) return ApiResponse<DepartmentResponseDto>.Failure(400, "Tên hoặc Mã đơn vị đã tồn tại.");

                Guid? resolvedParentId = null;

                if (user.Role == UserRole.Director || user.Role == UserRole.InstituteDirector)
                {
                    resolvedParentId = null;
                }
                else if (user.Role == UserRole.Manager)
                {
                    if (!user.DepartmentId.HasValue)
                        return ApiResponse<DepartmentResponseDto>.Failure(403, "Trưởng phòng cần phải gia nhập Trung tâm trước khi tạo Phòng ban trực thuộc.");

                    resolvedParentId = user.DepartmentId;
                }
                else
                {
                    return ApiResponse<DepartmentResponseDto>.Failure(403, "Bạn không có quyền khởi tạo đơn vị.");
                }

                var department = _mapper.Map<Department>(dto);
                department.Id = Guid.NewGuid();
                department.ParentId = resolvedParentId;
                department.CreatedAt = DateTime.UtcNow;

                user.DepartmentId = department.Id;

                await _unitOfWork.Departments.AddAsync(department);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                var response = _mapper.Map<DepartmentResponseDto>(department);
                response.ManagerName = user.Profile?.FullName ?? user.EmployeeCode;

                return ApiResponse<DepartmentResponseDto>.Success(response, "Khởi tạo đơn vị thành công.");
            });
        }

        public async Task<ApiResponse<DepartmentResponseDto>> UpdateDepartmentAsync(Guid id, Guid userId, DepartmentUpdateDto dto)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<DepartmentResponseDto>>(async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                var dept = await _unitOfWork.Departments.GetByIdAsync(id);

                if (dept == null) return ApiResponse<DepartmentResponseDto>.Failure(404, "Không tìm thấy đơn vị.");

                bool isAuthorized = false;

                if (user.Role == UserRole.Director)
                {
                    if (!dept.ParentId.HasValue) isAuthorized = true;
                }
                else if (user.Role == UserRole.Manager)
                {
                    if (user.DepartmentId == id) isAuthorized = true;
                }

                if (!isAuthorized) return ApiResponse<DepartmentResponseDto>.Failure(403, "Bạn không có quyền chỉnh sửa đơn vị này.");

                _mapper.Map(dto, dept);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<DepartmentResponseDto>.Success(_mapper.Map<DepartmentResponseDto>(dept), "Cập nhật thành công.");
            });
        }

        public async Task<ApiResponse<bool>> DeleteDepartmentAsync(Guid id, Guid userId)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<bool>>(async () =>
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null) return ApiResponse<bool>.Failure(401, "Người dùng không tồn tại.");

                var dept = await _unitOfWork.Departments.GetByIdAsync(id);
                if (dept == null) return ApiResponse<bool>.Failure(404, "Không tìm thấy đơn vị.");

                if (user.DepartmentId != id) return ApiResponse<bool>.Failure(403, "Bạn không có quyền xóa đơn vị mà bạn không trực tiếp quản lý.");
                if (user.Role == UserRole.Director && dept.ParentId.HasValue) return ApiResponse<bool>.Failure(403, "Giám đốc chỉ có quyền xóa Trung tâm.");

                if (user.Role == UserRole.Manager && !dept.ParentId.HasValue) return ApiResponse<bool>.Failure(403, "Trưởng phòng không thể xóa Trung tâm.");

                if (!dept.ParentId.HasValue)
                {
                    var hasSubDepartments = await _unitOfWork.Departments.AnyAsync(d => d.ParentId == id);
                    if (hasSubDepartments) return ApiResponse<bool>.Failure(400, "Không thể xóa Trung tâm khi vẫn còn các phòng ban con trực thuộc.");
                }

                bool hasOtherEmployees = await _unitOfWork.Users.AnyAsync(u => u.DepartmentId == id && u.Id != userId);
                if (hasOtherEmployees) return ApiResponse<bool>.Failure(400, "Không thể xóa đơn vị khi vẫn còn nhân viên khác đang tham gia.");

                user.DepartmentId = null;
                _unitOfWork.Departments.Remove(dept);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Success(true, "Xóa đơn vị thành công.");
            });
        }

        public async Task<ApiResponse<PagedResult<DepartmentResponseDto>>> GetCentersAsync(PaginationDto pagination, string? searchTerm)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<PagedResult<DepartmentResponseDto>>>(async () =>
            {
                var query = _unitOfWork.Departments.GetDepartmentsQueryable(searchTerm)
                                       .Where(d => d.ParentId == null);

                var paged = await query.ToPagedListAsync(pagination.Page, pagination.PageSize);
                var dtos = _mapper.Map<List<DepartmentResponseDto>>(paged.Items);

                return ApiResponse<PagedResult<DepartmentResponseDto>>.Success(new PagedResult<DepartmentResponseDto>
                {
                    Items = dtos,
                    TotalCount = paged.TotalCount,
                    Page = paged.Page,
                    PageSize = paged.PageSize
                });
            });
        }

        public async Task<ApiResponse<PagedResult<DepartmentResponseDto>>> GetSubDepartmentsAsync(Guid userId, Guid centerId, PaginationDto pagination, string? searchTerm)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<PagedResult<DepartmentResponseDto>>>(async () =>
            {
                IQueryable<Department> query;

                var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
                if (currentUser == null) return ApiResponse<PagedResult<DepartmentResponseDto>>.Failure(404, "Không tìm thấy người dùng.");

                // Coordinator
                if (currentUser.Role == UserRole.Coordinator)
                {
                    var centerIdOfCoordinator = currentUser.DepartmentId;
                    query = _unitOfWork.Departments.GetDepartmentsQueryable(searchTerm).Where(d => d.ParentId == centerIdOfCoordinator);
                }
                else
                {
                    var exists = await _unitOfWork.Departments.AnyAsync(d => d.Id == centerId);

                    if (!exists) return ApiResponse<PagedResult<DepartmentResponseDto>>.Failure(404, "Đơn vị cha không tồn tại.");

                    query = _unitOfWork.Departments.GetDepartmentsQueryable(searchTerm)
                        .Where(d => d.ParentId == centerId);
                }

                var paged = await query.ToPagedListAsync(
                    pagination.Page,
                    pagination.PageSize);

                var dtos = _mapper.Map<List<DepartmentResponseDto>>(paged.Items);

                return ApiResponse<PagedResult<DepartmentResponseDto>>.Success(
                    new PagedResult<DepartmentResponseDto>
                    {
                        Items = dtos,
                        TotalCount = paged.TotalCount,
                        Page = paged.Page,
                        PageSize = paged.PageSize
                    });
            });
        }
    }
}