using RealEstateApp.Application.DTOs.SaleTypes;
using RealEstateApp.Application.ViewModels.SaleTypes;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface ISaleTypeService : IGenericService<SaveSaleTypeViewModel, SaleTypeViewModel, SaleType>
    {
        Task<List<SaleTypeDto>> GetAllDto();
        Task<SaleTypeDto?> GetByIdDto(int id);
        Task<SaleTypeDto> AddDto(SaveSaleTypeDto dto);
        Task UpdateDto(SaveSaleTypeDto dto);
    }
}