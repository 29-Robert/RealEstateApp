using AutoMapper;
using RealEstateApp.Application.DTOs.Property;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IMapper _mapper;

        public PropertyService(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _mapper = mapper;
        }

        public async Task<List<PropertyDto>> GetAllAsync()
        {
            var properties = await _propertyRepository.GetAllWithIncludeAsync(
                "PropertyType", "SaleType", "PropertyImages");
            return _mapper.Map<List<PropertyDto>>(properties);
        }

        public async Task<PropertyDetailDto?> GetByIdAsync(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null) return null;
            return _mapper.Map<PropertyDetailDto>(property);
        }

        public async Task<PropertyDetailDto?> GetByCodeAsync(int code)
        {
            var property = await _propertyRepository.GetByCodeAsync(code);
            if (property == null) return null;
            return _mapper.Map<PropertyDetailDto>(property);
        }

        public async Task<List<PropertyDto>> GetAvailableForHomeAsync()
        {
            var properties = await _propertyRepository.GetAvailableOrderedByNewestAsync();
            return _mapper.Map<List<PropertyDto>>(properties);
        }

        public async Task<List<PropertyDto>> GetFilteredAsync(
            int? propertyTypeId,
            decimal? minPrice,
            decimal? maxPrice,
            int? rooms,
            int? bathrooms)
        {
            var properties = await _propertyRepository.GetFilteredAsync(
                propertyTypeId, minPrice, maxPrice, rooms, bathrooms);
            return _mapper.Map<List<PropertyDto>>(properties);
        }

        public async Task<List<PropertyDto>> GetByAgentAsync(string agentId, bool onlyAvailable)
        {
            var properties = await _propertyRepository.GetByAgentIdAsync(agentId, onlyAvailable);
            return _mapper.Map<List<PropertyDto>>(properties);
        }

        public async Task<PropertyDto> CreateAsync(SavePropertyDto dto)
        {
            var code = await _propertyRepository.GenerateUniqueCodeAsync();

            var property = new Property
            {
                Code = code.ToString(),
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                LandSize = (int)dto.Size,
                Rooms = dto.Rooms,
                Bathrooms = dto.Bathrooms,
                PropertyTypeId = dto.PropertyTypeId,
                SaleTypeId = dto.SaleTypeId,
                AgentId = dto.AgentId,
                Status = PropertyStatus.Disponible.ToString(),
                PropertyStatus = PropertyStatus.Disponible
            };

            property = await _propertyRepository.AddAsync(property);

            // Guardar las imágenes en base64 (el controller convierte los archivos a base64)
            bool isFirst = true;
            foreach (var base64Image in dto.NewImagesBase64)
            {
                var image = new PropertyImage
                {
                    PropertyId = property.Id,
                    ImagePath = base64Image,
                    IsMain = isFirst
                };
                await _propertyImageRepository.AddAsync(image);
                isFirst = false;
            }

            return _mapper.Map<PropertyDto>(property);
        }

        public async Task UpdateAsync(int id, SavePropertyDto dto)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null) return;

            property.Title = dto.Title;
            property.Description = dto.Description;
            property.Price = dto.Price;
            property.LandSize = (int)dto.Size;
            property.Rooms = dto.Rooms;
            property.Bathrooms = dto.Bathrooms;
            property.PropertyTypeId = dto.PropertyTypeId;
            property.SaleTypeId = dto.SaleTypeId;

            await _propertyRepository.UpdateAsync(property);

            // Eliminar imágenes que el usuario eliminó
            if (dto.ExistingImageIds != null && dto.ExistingImageIds.Any())
            {
                var existingImages = await _propertyImageRepository.GetByPropertyIdAsync(property.Id);
                var imagesToDelete = existingImages
                    .Where(img => !dto.ExistingImageIds.Contains(img.Id))
                    .ToList();

                foreach (var img in imagesToDelete)
                {
                    await _propertyImageRepository.DeleteAsync(img);
                }
            }

            // Agregar nuevas imágenes
            foreach (var base64Image in dto.NewImagesBase64)
            {
                var currentCount = await _propertyImageRepository.CountByPropertyIdAsync(property.Id);
                if (currentCount >= 4) break;

                var image = new PropertyImage
                {
                    PropertyId = property.Id,
                    ImagePath = base64Image,
                    IsMain = currentCount == 0
                };
                await _propertyImageRepository.AddAsync(image);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property != null)
            {
                await _propertyRepository.DeleteAsync(property);
            }
        }
    }
}
