using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Application.DTOs.Agent;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Infrastructure.Identity.Entities;


namespace RealEstateApp.Infrastructure.Identity.Services
{
 
    public class AgentService : IAgentService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public AgentService(UserManager<ApplicationUser> userManager, IPropertyRepository propertyRepository, IMapper mapper)
        {
            _userManager = userManager;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<List<AgentDto>> GetAllActiveOrderedByNameAsync()
        {
            var agents = await _userManager.GetUsersInRoleAsync(Roles.Agente.ToString());
            var activeAgents = agents.Where(a => a.IsActive).OrderBy(a => a.FirstName).ThenBy(a => a.LastName);

            var result = new List<AgentDto>();
            foreach (var agent in activeAgents)
            {
                var properties = await _propertyRepository.GetByAgentIdAsync(agent.Id);
                result.Add(new AgentDto
                {
                    Id = agent.Id,
                    FirstName = agent.FirstName,
                    LastName = agent.LastName,
                    Email = agent.Email ?? string.Empty,
                    PhotoPath = agent.PhotoPath ?? "",
                    IsActive = agent.IsActive,
                    PropertyCount = properties.Count
                });
            }

            return result;
        }

        public async Task<List<AgentDto>> SearchByNameAsync(string term)
        {
            var all = await GetAllActiveOrderedByNameAsync();
            return all.Where(a =>
                a.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                a.LastName.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<AgentDetailDto?> GetByIdAsync(string id)
        {
            var agent = await _userManager.FindByIdAsync(id);
            if (agent is null) return null;

            var isAgent = await _userManager.IsInRoleAsync(agent, Roles.Agente.ToString());
            if (!isAgent) return null;

            var properties = await _propertyRepository.GetByAgentIdAsync(id, onlyAvailable: true);

            return new AgentDetailDto
            {
                Id = agent.Id,
                FirstName = agent.FirstName,
                LastName = agent.LastName,
                Email = agent.Email ?? string.Empty,
                Phone = agent.PhoneNumber ?? string.Empty,
                PhotoPath = agent.PhotoPath ?? "",
                IsActive = agent.IsActive,
                PropertyCount = properties.Count,
                AvailableProperties = _mapper.Map<List<Application.DTOs.Property.PropertyDto>>(properties)
            };
        }

        /// <summary>Un agente inactivo no puede hacer login ni mostrar sus propiedades en la web publica.</summary>
        public async Task<bool> ChangeStatusAsync(string agentId, bool isActive)
        {
            var agent = await _userManager.FindByIdAsync(agentId);
            if (agent is null) return false;

            agent.IsActive = isActive;
            var result = await _userManager.UpdateAsync(agent);
            return result.Succeeded;
        }

        /// <summary>Al eliminar un agente se eliminan en cascada sus propiedades y datos relacionados.</summary>
        public async Task DeleteWithCascadeAsync(string agentId)
        {
            var agent = await _userManager.FindByIdAsync(agentId);
            if (agent is null) return;

            var properties = await _propertyRepository.GetByAgentIdAsync(agentId);
            foreach (var property in properties)
            {
                await _propertyRepository.DeleteAsync(property); // cascada via FKs configuradas en el DbContext
            }

            await _userManager.DeleteAsync(agent);
        }
    }
}