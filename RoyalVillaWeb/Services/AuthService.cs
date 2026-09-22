using RoyalVilla.Dto;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;

namespace RoyalVillaWeb.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private const string ApiEndpoint = "/api/auth";
        public AuthService(IHttpClientFactory httpClient, IConfiguration configuration, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor) : base(httpClient, tokenProvider, httpContextAccessor)
        {
        }

        public Task<T?> LoginAsync<T>(LoginRequestDto loginRequestDto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = ApiEndpoint + "/login",
                Data = loginRequestDto,
            }, withBearer: false);
        }

        public Task<T?> RefreshTokenAsync<T>(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = ApiEndpoint + "/refresh-token",
                Data = refreshTokenRequestDto,
            }, withBearer: false);
        }

        public Task<T?> RegisterAsync<T>(RegistrationRequestDto registerRequestDto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = ApiEndpoint + "/register",
                Data = registerRequestDto,
            }, withBearer: false);
        }
    }
}
