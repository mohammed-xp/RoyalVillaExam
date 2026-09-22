using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RoyalVilla_API.Services
{
    public class TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager, AppDbContext dbContext) : ITokenService
    {
        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var key = Encoding.ASCII.GetBytes(configuration.GetSection("JwtSettings:Secret").Value);

            var roles = await userManager.GetRolesAsync(user);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()?? "customer"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var refreshToken = Convert.ToBase64String(randomNumber);

            var exists = await dbContext.RefreshTokens.AnyAsync(u => u.RefreshTokenValue == refreshToken);

            if (exists)
            {
                return await GenerateRefreshTokenAsync();
            }

            return refreshToken;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshTokenId)
        {
            var storedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(u => u.RefreshTokenValue == refreshTokenId);

            if (storedToken == null)
            {
                return false;
            }
            storedToken.IsValid = false;
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task SaveRefreshTokenAsync(string userId, string jwtTokenId, string refreshToken, DateTime expiresAt)
        {
            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                JwtTokenId = jwtTokenId,
                RefreshTokenValue = refreshToken,
                ExpiresAt = expiresAt,
                IsValid = true
            };

            await dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
            await dbContext.SaveChangesAsync();
        }

        public async Task<(bool IsValid, string? UserId, string? TokenFamilyId, bool TokenReused)> ValidateRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(u => u.RefreshTokenValue == refreshToken);

            if(storedToken == null)
            {
                return (false, null, null, false);
            }

            //CRITICAL SECURITY CHECK: Token Reused Detection
            //If token exits but is marked as invalid, its mean someone tried to reuse it
            //This is a strong indicator of token theft
            if (!storedToken.IsValid)
            {
                // Revoke all tokens in THIS TOKEN FAMILY

                var refreshTokensEntity = await dbContext.RefreshTokens.Where(u => u.JwtTokenId == storedToken.JwtTokenId && u.UserId == storedToken.UserId).ToListAsync();

                if (refreshTokensEntity.Count > 0)
                {
                    foreach (var refreshTokeEntity in refreshTokensEntity)
                    {
                        refreshTokeEntity.IsValid = false;
                    }
                    await dbContext.SaveChangesAsync();
                }

                return (false, storedToken.UserId, storedToken.JwtTokenId, true);
            }

            if(storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return (false, storedToken.UserId, storedToken.JwtTokenId, false);
            }

            return (true, storedToken.UserId, storedToken.JwtTokenId, false);

        }
    }
}
