using RealEstateApp.Application.DTOs.Improvements;
using RealEstateApp.Application.ViewModels.Improvements;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IImprovementService : IGenericService<SaveImprovementViewModel, ImprovementViewModel, Improvement>
    {
        Task<List<ImprovementDto>> GetAllDto();
        Task<ImprovementDto?> GetByIdDto(int id);
        Task<ImprovementDto> AddDto(SaveImprovementDto dto);
        Task UpdateDto(SaveImprovementDto dto);
    }
}