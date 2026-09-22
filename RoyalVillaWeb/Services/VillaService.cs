using RoyalVilla.Dto;
using RoyalVillaWeb.Extensions;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;

namespace RoyalVillaWeb.Services
{
    public class VillaService : BaseService, IVillaService
    {
        private const string ApiEndpoint = $"/api/{SD.CurrentApiVersion}/villa";
        private readonly ITokenProvider _tokenProvider;
        public VillaService(IHttpClientFactory httpClient, IConfiguration configuration, ITokenProvider tokenProvider, IHttpContextAccessor httpContextAccessor) : base(httpClient, tokenProvider, httpContextAccessor)
        {
            _tokenProvider = tokenProvider;
        }

        public Task<T?> CreateAsync<T>(VillaCreateDto dto)
        {
            var formData = dto.ToMultipartFormData();
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = ApiEndpoint,
                Data = formData,
            });
        }

        public Task<T?> DeleteAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{ApiEndpoint}/{id}",
            });
        }

        public Task<T?> GetAllAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = ApiEndpoint,
                Token = _tokenProvider.GetAccessToken()
            });
        }

        public Task<T?> GetAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{ApiEndpoint}/{id}",
            });
        }

        public Task<T?> UpdateAsync<T>(VillaUpdateDto dto)
        {
            var formData = dto.ToMultipartFormData();
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Url = $"{ApiEndpoint}/{dto.Id}",
                Data = formData,
            });
        }
    }
}
