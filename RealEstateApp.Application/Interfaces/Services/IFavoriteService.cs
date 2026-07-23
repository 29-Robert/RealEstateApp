using RealEstateApp.Application.DTOs.Property;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IFavoriteService
    {
        Task ToggleFavoriteAsync(string clientId, int propertyId);
        Task<List<PropertyDto>> GetClientFavoritesAsync(string clientId);
        Task<List<int>> GetClientFavoritePropertyIdsAsync(string clientId);
    }
}