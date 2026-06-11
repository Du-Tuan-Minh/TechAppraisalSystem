using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<TokenResponse>> RegisterAsync(RegisterRequest request);
        Task<ApiResponse<TokenResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<TokenResponse>> RefreshTokenAsync(RefreshRequest request);
        Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
    }
}
