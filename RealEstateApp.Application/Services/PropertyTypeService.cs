using AutoMapper;
using RealEstateApp.Application.DTOs.PropertyTypes;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.PropertyTypes;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Services
{
    public class PropertyTypeService(IPropertyTypeRepository propertyTypeRepository, IMapper mapper) : GenericService<SavePropertyTypeViewModel, PropertyTypeViewModel, PropertyType>(propertyTypeRepository, mapper), IPropertyTypeService
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository = propertyTypeRepository;
        private readonly IMapper _mapper = mapper;

        public override async Task<List<PropertyTypeViewModel>> GetAllViewModel()
        {
            var list = await _propertyTypeRepository.GetAllWithIncludeAsync("Properties");
            return _mapper.Map<List<PropertyTypeViewModel>>(list);
        }

        public async Task<List<PropertyTypeDto>> GetAllDto()
        {
            var list = await _propertyTypeRepository.GetAllAsync();
            return _mapper.Map<List<PropertyTypeDto>>(list);
        }

        public async Task<PropertyTypeDto?> GetByIdDto(int id)
        {
            var entity = await _propertyTypeRepository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<PropertyTypeDto>(entity);
        }

        public async Task<PropertyTypeDto> AddDto(SavePropertyTypeDto dto)
        {
            var entity = _mapper.Map<PropertyType>(dto);
            entity = await _propertyTypeRepository.AddAsync(entity);
            return _mapper.Map<PropertyTypeDto>(entity);
        }

        public async Task UpdateDto(SavePropertyTypeDto dto)
        {
            var entity = _mapper.Map<PropertyType>(dto);
            await _propertyTypeRepository.UpdateAsync(entity);
        }
    }
}