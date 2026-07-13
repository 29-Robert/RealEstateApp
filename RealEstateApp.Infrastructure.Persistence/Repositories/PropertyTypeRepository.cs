using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyTypeRepository(ApplicationDbContext dbContext) : GenericRepository<PropertyType>(dbContext), IPropertyTypeRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
    }
}