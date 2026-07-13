using AutoMapper;
using RealEstateApp.Application.DTOs.SaleTypes;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.SaleTypes;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Services
{
    public class SaleTypeService(ISaleTypeRepository saleTypeRepository, IMapper mapper) : GenericService<SaveSaleTypeViewModel, SaleTypeViewModel, SaleType>(saleTypeRepository, mapper), ISaleTypeService
    {
        private readonly ISaleTypeRepository _saleTypeRepository = saleTypeRepository;
        private readonly IMapper _mapper = mapper;

        public override async Task<List<SaleTypeViewModel>> GetAllViewModel()
        {
            var list = await _saleTypeRepository.GetAllWithIncludeAsync("Properties");
            return _mapper.Map<List<SaleTypeViewModel>>(list);
        }

        public async Task<List<SaleTypeDto>> GetAllDto()
        {
            var list = await _saleTypeRepository.GetAllAsync();
            return _mapper.Map<List<SaleTypeDto>>(list);
        }

        public async Task<SaleTypeDto?> GetByIdDto(int id)
        {
            var entity = await _saleTypeRepository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<SaleTypeDto>(entity);
        }

        public async Task<SaleTypeDto> AddDto(SaveSaleTypeDto dto)
        {
            var entity = _mapper.Map<SaleType>(dto);
            entity = await _saleTypeRepository.AddAsync(entity);
            return _mapper.Map<SaleTypeDto>(entity);
        }

        public async Task UpdateDto(SaveSaleTypeDto dto)
        {
            var entity = _mapper.Map<SaleType>(dto);
            await _saleTypeRepository.UpdateAsync(entity);
        }
    }
}