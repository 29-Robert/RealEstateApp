using Microsoft.EntityFrameworkCore;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyImageRepository(ApplicationDbContext dbContext)
        : GenericRepository<PropertyImage>(dbContext), IPropertyImageRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<List<PropertyImage>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.PropertyImages
                .Where(pi => pi.PropertyId == propertyId)
                .ToListAsync();
        }

        public async Task<int> CountByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.PropertyImages
                .CountAsync(pi => pi.PropertyId == propertyId);
        }
    }
}
