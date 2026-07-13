using RealEstateApp.Application.DTOs.PropertyTypes;
using RealEstateApp.Application.ViewModels.PropertyTypes;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IPropertyTypeService : IGenericService<SavePropertyTypeViewModel, PropertyTypeViewModel, PropertyType>
    {
        Task<List<PropertyTypeDto>> GetAllDto();
        Task<PropertyTypeDto?> GetByIdDto(int id);
        Task<PropertyTypeDto> AddDto(SavePropertyTypeDto dto);
        Task UpdateDto(SavePropertyTypeDto dto);
    }
}