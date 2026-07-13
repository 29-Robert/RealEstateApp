using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class ImprovementRepository(ApplicationDbContext dbContext) : GenericRepository<Improvement>(dbContext), IImprovementRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
    }
}