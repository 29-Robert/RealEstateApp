using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class OfferRepository(ApplicationDbContext dbContext) : GenericRepository<Offer>(dbContext), IOfferRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
    }
}
