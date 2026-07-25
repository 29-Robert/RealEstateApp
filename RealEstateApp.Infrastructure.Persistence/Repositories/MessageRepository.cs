using Microsoft.EntityFrameworkCore;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class MessageRepository(ApplicationDbContext dbContext)
        : GenericRepository<Message>(dbContext), IMessageRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<List<Message>> GetChatHistoryAsync(string clientId, string agentId, int propertyId)
        {
            return await _dbContext.Messages
                .Where(m => m.ClientId == clientId && m.AgentId == agentId && m.PropertyId == propertyId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessagesByAgentAsync(string agentId)
        {
            return await _dbContext.Messages
                .Where(m => m.AgentId == agentId)
                .Include(m => m.Property)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }
    }
}