using RoyalVilla.Dto;

namespace RoyalVillaWeb.Services.IServices
{
    public interface IAuthService
    {
        Task<T?> LoginAsync<T>(LoginRequestDto loginRequestDto);
        Task<T?> RegisterAsync<T>(RegistrationRequestDto registerRequestDto);
        Task<T?> RefreshTokenAsync<T>(RefreshTokenRequestDto refreshTokenRequestDto);
    }
}
