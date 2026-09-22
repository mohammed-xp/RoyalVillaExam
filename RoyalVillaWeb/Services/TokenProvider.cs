using Microsoft.AspNetCore.Authentication.Cookies;
using RoyalVilla.Dto;
using RoyalVillaWeb.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoyalVillaWeb.Services
{
    public class TokenProvider(IHttpContextAccessor httpContextAccessor) : ITokenProvider
    {
        public void ClearToken()
        {
            httpContextAccessor.HttpContext?.Session.Remove(SD.SessionAccessToken);
            httpContextAccessor.HttpContext?.Session.Remove(SD.SessionRefreshToken);
        }

        public ClaimsPrincipal? CreatePrincipalFromJwtToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                var emailClaim = jwt.Claims.FirstOrDefault(u => u.Type == "email");
                if (emailClaim != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, emailClaim.Value));
                }

                var roleClaim = jwt.Claims.FirstOrDefault(u => u.Type == "role");
                if (roleClaim != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                }

                var nameClaim = jwt.Claims.FirstOrDefault(u => u.Type == "name");
                if (nameClaim != null)
                {
                    identity.AddClaim(new Claim("FullName", nameClaim.Value));
                }

                return new ClaimsPrincipal(identity);

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string? GetAccessToken()
        {
            return httpContextAccessor.HttpContext?.Session.GetString(SD.SessionAccessToken);
        }

        public string? GetRefreshToken()
        {
            return httpContextAccessor.HttpContext?.Session.GetString(SD.SessionAccessToken);
        }

        public void SetToken(string accessToken, string refreshToken)
        {
            httpContextAccessor.HttpContext?.Session.SetString(SD.SessionAccessToken, accessToken);
            httpContextAccessor.HttpContext?.Session.SetString(SD.SessionRefreshToken, refreshToken);
        }
    }
}
