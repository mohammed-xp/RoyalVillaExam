using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla.Dto;
using RoyalVilla_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Services
{
    public class AuthService(
        AppDbContext dbContext,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService) : IAuthService
    {
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await dbContext.ApplicationUsers.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<TokenDto?> LoginAsync(LoginRequestDto loginRequestDto)
        {
            try
            {
                var user = await dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDto.Email.ToLower());
                if (user == null)
                {
                    return null; // user not found
                }

                var isValid = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                if (!isValid)
                {
                    return null; // invalid password
                }

                // Generate Token
                var token = await tokenService.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jwtTokenId = jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti)?.Value;

                // Generate New REfresh Token
                var newRefreshToken = await tokenService.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await tokenService.SaveRefreshTokenAsync(user.Id, jwtTokenId, newRefreshToken, refreshTokenExpiry);

                TokenDto tokenDto = new TokenDto
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };

                return tokenDto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while registering the user", ex);
            }
        }

        public async Task<UserDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            try
            {
                if (await IsEmailExistsAsync(registrationRequestDto.Email))
                {
                    throw new InvalidOperationException($"A user with the email '{registrationRequestDto.Email}' already exists");
                }

                ApplicationUser user = new()
                {
                    Email = registrationRequestDto.Email,
                    Name = registrationRequestDto.Name,
                    UserName = registrationRequestDto.Email,
                    NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, registrationRequestDto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"User registration failed: {errors}");
                }

                var role = string.IsNullOrEmpty(registrationRequestDto.Role) ? "customer" : registrationRequestDto.Role;

                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }

                await userManager.AddToRoleAsync(user, role);

                var userDto = mapper.Map<UserDto>(user);
                userDto.Role = role;

                return userDto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while registering the user", ex);
            }
        }

        public async Task<TokenDto?> RefreshAccessTokenAsync(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            try
            {
                if(string.IsNullOrEmpty(refreshTokenRequestDto.RefreshToken))
                {
                    return null;
                }

                //validate refresh token
                var (isValid, userId, tokenFamilyId, tokenReused) = await tokenService.ValidateRefreshTokenAsync(refreshTokenRequestDto.RefreshToken);

                //Token reuse detected
                if (tokenReused)
                {
                    return null;
                }

                //Token is invalid or expire
                if(!isValid || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenFamilyId))
                {
                    return null;
                }

                //Get user
                var user = await dbContext.ApplicationUsers.FindAsync(userId);

                if(user == null)
                {
                    return null;
                }

                //Revoke old refresh token
                await tokenService.RevokeRefreshTokenAsync(refreshTokenRequestDto.RefreshToken);

                //Generate new refresh & access token
                var token = await tokenService.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var newRefreshToken = await tokenService.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await tokenService.SaveRefreshTokenAsync(user.Id, tokenFamilyId, newRefreshToken, refreshTokenExpiry);

                TokenDto tokenDto = new TokenDto
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };

                return tokenDto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while 'Refresh Access Token'", ex);
            }
        }
    }
}
