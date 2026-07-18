using RealEstateApp.Application.DTOs.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IAgentService
    {
        Task<List<AgentDto>> GetAllActiveOrderedByNameAsync();
        Task<List<AgentDto>> SearchByNameAsync(string term);
        Task<AgentDetailDto?> GetByIdAsync(string id);
        Task<bool> ChangeStatusAsync(string agentId, bool isActive);
        Task DeleteWithCascadeAsync(string agentId);
    }

}
