using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Interfaces.Repositories
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        Task<List<Message>> GetChatHistoryAsync(string clientId, string agentId, int propertyId);
        Task<List<Message>> GetMessagesByAgentAsync(string agentId);
    }
}