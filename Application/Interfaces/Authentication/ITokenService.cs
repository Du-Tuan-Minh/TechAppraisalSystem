using Domain.Entities;

namespace Application.Interfaces.Authentication
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}