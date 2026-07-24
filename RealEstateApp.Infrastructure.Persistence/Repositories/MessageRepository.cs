using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class MessageRepository(ApplicationDbContext dbContext) : GenericRepository<Message>(dbContext), IMessageRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
    }
}
