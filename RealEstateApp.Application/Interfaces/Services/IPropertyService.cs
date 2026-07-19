using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealEstateApp.Application.DTOs.Property;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IPropertyService
    {
        Task<List<PropertyDto>> GetAllAsync();
        Task<PropertyDetailDto?> GetByIdAsync(int id);
        Task<PropertyDetailDto?> GetByCodeAsync(int code);
        Task<List<PropertyDto>> GetAvailableForHomeAsync();
        Task<List<PropertyDto>> GetFilteredAsync(int? propertyTypeId, decimal? minPrice, decimal? maxPrice, int? rooms, int? bathrooms);
        Task<List<PropertyDto>> GetByAgentAsync(string agentId, bool onlyAvailable);
        Task<PropertyDto> CreateAsync(SavePropertyDto dto);
        Task UpdateAsync(int id, SavePropertyDto dto);
        Task DeleteAsync(int id);
    }
}
