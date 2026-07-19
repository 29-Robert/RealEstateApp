using Microsoft.EntityFrameworkCore;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyRepository(ApplicationDbContext dbContext)
        : GenericRepository<Property>(dbContext), IPropertyRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<List<Property>> GetAvailableOrderedByNewestAsync()
        {
            return await _dbContext.Properties
                .Where(p => p.PropertyStatus == PropertyStatusEnum.Disponible)
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.PropertyImages)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<Property?> GetByCodeAsync(int code)
        {
            return await _dbContext.Properties
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.PropertyImages)
                .Include(p => p.Improvements)
                .FirstOrDefaultAsync(p => p.Code == code.ToString());
        }

        public async Task<int> GenerateUniqueCodeAsync()
        {
            var rnd = new Random();
            int code;
            bool exists;
            do
            {
                code = rnd.Next(100000, 999999);
                exists = await _dbContext.Properties.AnyAsync(p => p.Code == code.ToString());
            } while (exists);

            return code;
        }

        public async Task<List<Property>> GetByAgentIdAsync(string agentId, bool onlyAvailable = false)
        {
            var query = _dbContext.Properties
                .Where(p => p.AgentId == agentId)
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.PropertyImages)
                .AsQueryable();

            if (onlyAvailable)
            {
                query = query.Where(p => p.PropertyStatus == PropertyStatusEnum.Disponible);
            }

            return await query.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<List<Property>> GetFilteredAsync(
            int? propertyTypeId,
            decimal? minPrice,
            decimal? maxPrice,
            int? rooms,
            int? bathrooms)
        {
            var query = _dbContext.Properties
                .Where(p => p.PropertyStatus == PropertyStatusEnum.Disponible)
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.PropertyImages)
                .AsQueryable();

            if (propertyTypeId.HasValue)
                query = query.Where(p => p.PropertyTypeId == propertyTypeId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (rooms.HasValue)
                query = query.Where(p => p.Rooms == rooms.Value);

            if (bathrooms.HasValue)
                query = query.Where(p => p.Bathrooms == bathrooms.Value);

            return await query.OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<bool> ChangeStatusAsync(int propertyId, PropertyStatusEnum status)
        {
            var property = await _dbContext.Properties.FindAsync(propertyId);
            if (property == null) return false;

            property.PropertyStatus = status;
            _dbContext.Entry(property).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<(int available, int sold)> GetKpiCountsAsync()
        {
            var available = await _dbContext.Properties
                .CountAsync(p => p.PropertyStatus == PropertyStatusEnum.Disponible);
            var sold = await _dbContext.Properties
                .CountAsync(p => p.PropertyStatus == PropertyStatusEnum.Vendida);
            return (available, sold);
        }

        public override async Task<Property?> GetByIdAsync(int id)
        {
            return await _dbContext.Properties
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.PropertyImages)
                .Include(p => p.Improvements)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
