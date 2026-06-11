using Application.Common;
using Application.DTOs;
using Application.Interfaces.Authentication;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using static BCrypt.Net.BCrypt;

namespace Application.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly ITokenService _tokenService;

        public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService) : base(unitOfWork)
        {
            _tokenService = tokenService;
        }

        public async Task<ApiResponse<TokenResponse>> RegisterAsync(RegisterRequest request)
        {
            if (await _unitOfWork.Users.AnyAsync(u => u.EmployeeCode == request.EmployeeCode))
                return ApiResponse<TokenResponse>.Failure(400, "This EmployeeCode has already been used.");

            var user = new User
            {
                EmployeeCode = request.EmployeeCode,
                HashPassword = HashPassword(request.Password),
                Role = UserRole.Staff,
                Profile = new Profile
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName
                }
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<TokenResponse>.Success(null!, "Account registration successful.");
        }

        public async Task<ApiResponse<TokenResponse>> LoginAsync(LoginRequest request)
        {
            var users = await _unitOfWork.Users.FindAsync(
                u => u.EmployeeCode == request.EmployeeCode,
                u => u.Profile!);

            var user = users.FirstOrDefault();

            if (user == null || !Verify(request.Password, user.HashPassword))
                return ApiResponse<TokenResponse>.Failure(401, "Incorrect EmployeeCode or password.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<TokenResponse>.Success(
                new TokenResponse(accessToken, refreshToken), "Login successful."
            );
        }

        public async Task<ApiResponse<TokenResponse>> RefreshTokenAsync(RefreshRequest request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.RefreshToken == request.RefreshToken);
            var user = users.FirstOrDefault();

            if (user == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                return ApiResponse<TokenResponse>.Failure(401, "Your login session has expired. Please log in again.");

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<TokenResponse>.Success(new TokenResponse(newAccessToken, newRefreshToken));
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.RefreshToken == refreshToken);
            var user = users.FirstOrDefault();

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }

            return ApiResponse<bool>.Success(true, "Signed out.");
        }
    }
}