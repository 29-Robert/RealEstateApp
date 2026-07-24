using RealEstateApp.Application.DTOs.Message;
using RealEstateApp.Application.ViewModels.Message;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IMessageService
    {
        Task SendMessageAsync(SendMessageViewModel vm, string clientId);
        Task ReplyMessageAsync(ReplyMessageViewModel vm, string agentId);
        Task<List<MessageDto>> GetConversationAsync(int propertyId, string clientId);
        Task<List<MessageDto>> GetAgentMessagesAsync(string agentId);
    }
}
