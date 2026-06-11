using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces.Authentication;
using DotNetEnv;

namespace Infrastructure.Persistence
{
        public class TokenService: ITokenService
    {
            public string GenerateAccessToken(User user)
            {
            var secret = Env.GetString("JWT_SECRET");
            var issuer = Env.GetString("JWT_ISSUER");
            var audience = Env.GetString("JWT_AUDIENCE");
            var expiryMinutes = Env.GetInt("JWT_EXPIRY_MINUTES");

            if (string.IsNullOrEmpty(secret))
                    throw new InvalidOperationException("JWT_SECRET is not configured.");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.EmployeeCode),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
            };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(securityToken);
            }

            public string GenerateRefreshToken()
            {
                return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            }
        }
    }
