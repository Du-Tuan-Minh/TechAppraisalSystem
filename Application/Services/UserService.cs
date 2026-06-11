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
    public class UserService : BaseService, IUserService
    {
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
        {
            _mapper = mapper;
        }

        public async Task<ApiResponse<bool>> UpdateUserAccountAsync(Guid userId, UserUpdateAccountDto dto)
        {
            var (user, error) = await GetUserOrErrorAsync<bool>(userId);
            if (error != null) return error;
            _mapper.Map(dto, user);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Account status update successful.");
        }

        public async Task<ApiResponse<bool>> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var (user, error) = await GetUserOrErrorAsync<bool>(userId, u => u.Profile!);
            if (error != null) return error;
            _mapper.Map(dto, user.Profile);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Personal information updated successfully.");
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var (user, error) = await GetUserOrErrorAsync<bool>(userId);
            if (error != null) return error;

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.HashPassword))
                return ApiResponse<bool>.Failure(400, "The old password is incorrect.");

            if (dto.NewPassword == dto.OldPassword)
                return ApiResponse<bool>.Failure(400, "The new password cannot be the same as the old password.");

            user.HashPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Password changed successfully.");
        }

        public async Task<ApiResponse<bool>> RequestRolePromotionAsync(Guid userId, UserRole requestedRole, string reason)
        {
            var (user, error) = await GetUserOrErrorAsync<bool>(userId);
            if (error != null) return error;

            var admins = await _unitOfWork.Users.GetAdminsAsync();
            foreach (var admin in admins)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    Title = "Yêu cầu thăng cấp quyền",
                    Content = $"User {user.EmployeeCode} yêu cầu quyền {requestedRole}. Lý do: {reason}",
                    Type = NotificationType.System
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Yêu cầu đã được gửi tới Ban quản trị.");
        }

        public async Task<ApiResponse<PagedResult<UserResponseDto>>> GetUsersPagedAsync(UserFilterDto filter, Guid currentUserId)
        {
            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (currentUser == null) return ApiResponse<PagedResult<UserResponseDto>>.Failure(404, "Người dùng không tồn tại.");

            bool includeSubDepartments = false;
            bool staffOnly = false;

            switch (currentUser.Role)
            {
                case UserRole.Admin:
                    includeSubDepartments = filter.DepartmentId.HasValue;
                    break;

                case UserRole.Director:
                    if (filter.DepartmentId.HasValue)
                    {
                        if (filter.DepartmentId == currentUser.DepartmentId)
                        {
                            includeSubDepartments = true;
                        }
                        else
                        {
                            var isValidChild = await _unitOfWork.Departments.AnyAsync(d =>
                                d.Id == filter.DepartmentId &&
                                d.ParentId == currentUser.DepartmentId);

                            if (!isValidChild)
                            {
                                return ApiResponse<PagedResult<UserResponseDto>>
                                    .Failure(403, "Không cùng phòng ban.");
                            }

                            includeSubDepartments = false;
                        }
                    }
                    else
                    {
                        filter.DepartmentId = currentUser.DepartmentId;
                        includeSubDepartments = true;
                    }

                    break;

                case UserRole.Coordinator:
                    staffOnly = true;
                    filter.Role = null;
                    if (filter.DepartmentId.HasValue)
                    {
                        if (filter.DepartmentId == currentUser.DepartmentId)
                        {
                            includeSubDepartments = true;
                        }
                        else
                        {
                            var isValidChild = await _unitOfWork.Departments.AnyAsync(d =>
                                d.Id == filter.DepartmentId &&
                                d.ParentId == currentUser.DepartmentId);

                            if (!isValidChild)
                            {
                                return ApiResponse<PagedResult<UserResponseDto>>
                                    .Failure(403, "Không cùng phòng ban.");
                            }

                            includeSubDepartments = false;
                        }
                    }
                    else
                    {
                        filter.DepartmentId = currentUser.DepartmentId;
                        includeSubDepartments = true;
                    }

                    break;

                case UserRole.Manager:
                case UserRole.Staff:
                    if (filter.DepartmentId.HasValue && filter.DepartmentId != currentUser.DepartmentId)
                    {
                        return ApiResponse<PagedResult<UserResponseDto>>.Failure(403, "Không cùng phòng ban.");
                    }
                    filter.DepartmentId = currentUser.DepartmentId;
                    includeSubDepartments = false;
                    break;

                default:
                    return ApiResponse<PagedResult<UserResponseDto>>.Failure(403, "Bạn không có quyền truy cập.");
            }

            var pagedUsers = await _unitOfWork.Users.GetUsersFilteredAsync(
                filter.DepartmentId,
                filter.Role,
                filter.IsActive,
                filter.SearchTerm,
                filter.Page,
                filter.PageSize,
                includeSubDepartments,
                staffOnly);

            return ApiResponse<PagedResult<UserResponseDto>>.Success(_mapper.Map<PagedResult<UserResponseDto>>(pagedUsers));
        }

        public async Task<ApiResponse<UserDetailResponseDto>> GetUserDetailAsync(Guid userId)
        {
            var (user, error) = await GetUserOrErrorAsync<UserDetailResponseDto>(userId);
            if (error != null) return error;
            var response = _mapper.Map<UserDetailResponseDto>(user);

            return ApiResponse<UserDetailResponseDto>.Success(response);
        }

        public async Task<ApiResponse<UserDetailResponseDto>> GetMyProfileAsync(Guid userId)
        {
            var (user, error) = await GetUserOrErrorAsync<UserDetailResponseDto>(userId, u => u.Profile!, u => u.Department!);
            if (error != null) return error;
            return ApiResponse<UserDetailResponseDto>.Success(_mapper.Map<UserDetailResponseDto>(user));
        }

        public async Task<ApiResponse<PagedResult<UserResponseDto>>> GetSeniorCenterAsync(PaginationDto pagination, string? searchTerm)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync<ApiResponse<PagedResult<UserResponseDto>>>(async () =>
            {
                var query = _unitOfWork.Users.GetSeniorCentersQueryable(searchTerm);

                var paged = await query.ToPagedListAsync(pagination.Page, pagination.PageSize);
                var dtos = _mapper.Map<List<UserResponseDto>>(paged.Items);

                return ApiResponse<PagedResult<UserResponseDto>>.Success(new PagedResult<UserResponseDto>
                {
                    Items = dtos,
                    TotalCount = paged.TotalCount,
                    Page = paged.Page,
                    PageSize = paged.PageSize
                });
            });
        }

        public async Task<ApiResponse<PagedResult<DocumentTypeStatisticDto>>> GetTopRejectedDocumentsAsync(Guid managerId, PaginationDto pagination)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var result = await _unitOfWork.Users.GetTopRejectedDocumentTypesAsync(managerId, pagination);
                return ApiResponse<PagedResult<DocumentTypeStatisticDto>>.Success(result);
            });
        }

        public async Task<ApiResponse<PagedResult<UserAppraisalAssigneeDto>>> GetDepartmentAppraisalWorkloadsAsync(PaginationDto pagination, string? searchTerm)
        {
            return await _unitOfWork.ExecuteWithStrategyAsync(async () =>
            {
                var users = await _unitOfWork.Users.GetDepartmentAppraisalWorkloadsAsync(pagination, searchTerm);

                var items = users.Items.Select(user =>
                {
                    var dto = _mapper.Map<UserAppraisalAssigneeDto>(user);

                    dto.TotalDocuments = user.Role == UserRole.Manager
                        ? user.ManagedAssignments
                            .Where(a =>
                                (a.Status == AssignmentStatus.Pending ||
                                 a.Status == AssignmentStatus.InReview) &&
                                (a.Document.Status == DocumentStatus.AppraisalPending ||
                                 a.Document.Status == DocumentStatus.Appraising))
                            .Select(a => a.DocumentId)
                            .Distinct()
                            .Count()

                        : user.AssignedReviews
                            .Where(r =>
                                (r.Status == ReviewerStatus.Pending ||
                                 r.Status == ReviewerStatus.Reviewing) &&
                                (r.Assignment.Document.Status == DocumentStatus.AppraisalPending ||
                                 r.Assignment.Document.Status == DocumentStatus.Appraising))
                            .Select(r => r.Assignment.DocumentId)
                            .Distinct()
                            .Count();

                    return dto;
                }).ToList();

                var result = new PagedResult<UserAppraisalAssigneeDto>
                {
                    Items = items,
                    TotalCount = users.TotalCount,
                    Page = users.Page,
                    PageSize = users.PageSize
                };

                return ApiResponse<PagedResult<UserAppraisalAssigneeDto>>.Success(result);
            });
        }
    }
}