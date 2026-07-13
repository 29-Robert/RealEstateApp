using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class SaleTypeRepository(ApplicationDbContext dbContext) : GenericRepository<SaleType>(dbContext), ISaleTypeRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
    }
}