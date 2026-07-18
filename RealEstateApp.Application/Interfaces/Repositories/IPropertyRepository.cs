using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.Interfaces.Repositories
{
    public interface IPropertyRepository : IGenericRepository<Property>
    {
        Task<List<Property>> GetAvailableOrderedByNewestAsync();
        Task<Property?> GetByCodeAsync(int code);
        Task<int> GenerateUniqueCodeAsync();
        Task<List<Property>> GetByAgentIdAsync(string agentId, bool onlyAvailable = false);
        Task<List<Property>> GetFilteredAsync(int? propertyTypeId, decimal? minPrice, decimal? maxPrice, int? rooms, int? bathrooms);
        Task<bool> ChangeStatusAsync(int propertyId, PropertyStatusEnum status);
        Task<(int available, int sold)> GetKpiCountsAsync();
    }
}
