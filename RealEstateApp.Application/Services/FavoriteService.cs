using AutoMapper;
using RealEstateApp.Application.DTOs.Property;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IGenericRepository<Favorite> _favoriteRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public FavoriteService(
            IGenericRepository<Favorite> favoriteRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        // Alterna entre agregar o quitar de favoritos (Toggle)
        public async Task ToggleFavoriteAsync(string clientId, int propertyId)
        {
            var favorites = await _favoriteRepository.GetAllAsync();
            var existingFavorite = favorites.FirstOrDefault(f => f.ClientId == clientId && f.PropertyId == propertyId);

            if (existingFavorite != null)
            {
                // Si ya existe en favoritos, lo eliminamos
                await _favoriteRepository.DeleteAsync(existingFavorite);
            }
            else
            {
                // Si no existe, lo creamos
                var newFavorite = new Favorite
                {
                    ClientId = clientId,
                    PropertyId = propertyId
                };
                await _favoriteRepository.AddAsync(newFavorite);
            }
        }

        // Obgiene las propiedades completas marcadas como favoritas por el cliente
        public async Task<List<PropertyDto>> GetClientFavoritesAsync(string clientId)
        {
            var favorites = await _favoriteRepository.GetAllAsync();
            var clientFavoritePropertyIds = favorites
                .Where(f => f.ClientId == clientId)
                .Select(f => f.PropertyId)
                .ToList();

            var allProperties = await _propertyRepository.GetAllAsync();
            var favoriteProperties = allProperties
                .Where(p => clientFavoritePropertyIds.Contains(p.Id) && p.PropertyStatus == PropertyStatus.Disponible)
                .ToList();

            // Mapeo corregido con AutoMapper
            var propertyDtos = _mapper.Map<List<PropertyDto>>(favoriteProperties);

            // Marcamos IsFavorite = true en todos los DTOs devueltos
            foreach (var dto in propertyDtos)
            {
                dto.IsFavorite = true;
            }

            return propertyDtos;
        }

        // Obtiene solo los IDs de las propiedades favoritas del cliente (ideal para renderizar los corazones en el Home)
        public async Task<List<int>> GetClientFavoritePropertyIdsAsync(string clientId)
        {
            var favorites = await _favoriteRepository.GetAllAsync();
            return favorites
                .Where(f => f.ClientId == clientId)
                .Select(f => f.PropertyId)
                .ToList();
        }
    }
}