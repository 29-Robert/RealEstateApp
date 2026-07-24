using AutoMapper;
using RealEstateApp.Application.DTOs.Message;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Message;
using RealEstateApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public MessageService(
            IMessageRepository messageRepository,
            IAccountService accountService,
            IMapper mapper)
        {
            _messageRepository = messageRepository;
            _accountService = accountService;
            _mapper = mapper;
        }

        public async Task SendMessageAsync(SendMessageViewModel vm, string clientId)
        {
            var message = new Message
            {
                PropertyId = vm.PropertyId,
                ClientId = clientId,
                AgentId = vm.AgentId,
                Content = vm.Content,
                SentAt = DateTime.Now,
                IsFromClient = true
            };

            await _messageRepository.AddAsync(message);
        }

        public async Task ReplyMessageAsync(ReplyMessageViewModel vm, string agentId)
        {
            var message = new Message
            {
                PropertyId = vm.PropertyId,
                ClientId = vm.ClientId,
                AgentId = agentId,
                Content = vm.Content,
                SentAt = DateTime.Now,
                IsFromClient = false
            };

            await _messageRepository.AddAsync(message);
        }

        public async Task<List<MessageDto>> GetConversationAsync(int propertyId, string clientId)
        {
            var messages = await _messageRepository.GetAllAsync();
            var conversation = messages
                .Where(m => m.PropertyId == propertyId && m.ClientId == clientId)
                .OrderBy(m => m.SentAt)
                .ToList();

            if (!conversation.Any())
            {
                return new List<MessageDto>();
            }

            var firstMsg = conversation.First();
            string clientName = "Cliente";
            string agentName = "Agente";

            var client = await _accountService.GetUserByIdAsync(firstMsg.ClientId);
            if (client != null)
            {
                clientName = $"{client.FirstName} {client.LastName}";
            }

            var agent = await _accountService.GetUserByIdAsync(firstMsg.AgentId);
            if (agent != null)
            {
                agentName = $"{agent.FirstName} {agent.LastName}";
            }

            var dtos = _mapper.Map<List<MessageDto>>(conversation);
            foreach (var dto in dtos)
            {
                dto.ClientName = clientName;
                dto.AgentName = agentName;
            }

            return dtos;
        }

        public async Task<List<MessageDto>> GetAgentMessagesAsync(string agentId)
        {
            var messages = await _messageRepository.GetAllAsync();
            var agentMessages = messages
                .Where(m => m.AgentId == agentId)
                .OrderBy(m => m.SentAt)
                .ToList();

            if (!agentMessages.Any())
            {
                return new List<MessageDto>();
            }

            var dtos = _mapper.Map<List<MessageDto>>(agentMessages);
            var userCache = new Dictionary<string, string>();

            var agent = await _accountService.GetUserByIdAsync(agentId);
            string agentName = agent != null ? $"{agent.FirstName} {agent.LastName}" : "Agente";

            foreach (var dto in dtos)
            {
                dto.AgentName = agentName;
                if (!userCache.ContainsKey(dto.ClientId))
                {
                    var client = await _accountService.GetUserByIdAsync(dto.ClientId);
                    userCache[dto.ClientId] = client != null ? $"{client.FirstName} {client.LastName}" : "Cliente";
                }
                dto.ClientName = userCache[dto.ClientId];
            }

            return dtos;
        }
    }
}
