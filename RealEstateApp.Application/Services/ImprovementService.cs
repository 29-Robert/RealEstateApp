using AutoMapper;
using RealEstateApp.Application.DTOs.Improvements;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Improvements;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Services
{
    public class ImprovementService(IImprovementRepository improvementRepository, IMapper mapper) : GenericService<SaveImprovementViewModel, ImprovementViewModel, Improvement>(improvementRepository, mapper), IImprovementService
    {
        private readonly IImprovementRepository _improvementRepository = improvementRepository;
        private readonly IMapper _mapper = mapper;

        public override async Task<List<ImprovementViewModel>> GetAllViewModel()
        {
            var list = await _improvementRepository.GetAllWithIncludeAsync("Properties");
            return _mapper.Map<List<ImprovementViewModel>>(list);
        }

        public async Task<List<ImprovementDto>> GetAllDto()
        {
            var list = await _improvementRepository.GetAllAsync();
            return _mapper.Map<List<ImprovementDto>>(list);
        }

        public async Task<ImprovementDto?> GetByIdDto(int id)
        {
            var entity = await _improvementRepository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<ImprovementDto>(entity);
        }

        public async Task<ImprovementDto> AddDto(SaveImprovementDto dto)
        {
            var entity = _mapper.Map<Improvement>(dto);
            entity = await _improvementRepository.AddAsync(entity);
            return _mapper.Map<ImprovementDto>(entity);
        }

        public async Task UpdateDto(SaveImprovementDto dto)
        {
            var entity = _mapper.Map<Improvement>(dto);
            await _improvementRepository.UpdateAsync(entity);
        }
    }
}