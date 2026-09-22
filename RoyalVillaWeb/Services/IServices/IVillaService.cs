using RoyalVilla.Dto;

namespace RoyalVillaWeb.Services.IServices
{
    public interface IVillaService
    {
        Task<T?> GetAllAsync<T>();
        Task<T?> GetAsync<T>(int id);
        Task<T?> CreateAsync<T>(VillaCreateDto dto);
        Task<T?> UpdateAsync<T>(VillaUpdateDto dto);
        Task<T?> DeleteAsync<T>(int id);
    }
}
