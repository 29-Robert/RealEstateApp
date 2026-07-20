using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.DTOs.Agent;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Presentation.WebApi.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AgentsController : ControllerBase
    {
        private readonly IAgentService _agentService;
        private readonly IPropertyService _propertyService;

        public AgentsController(IAgentService agentService, IPropertyService propertyService)
        {
            _agentService = agentService;
            _propertyService = propertyService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AgentDto>>> List()
        {
            var result = await _agentService.GetAllActiveOrderedByNameAsync();
            return result.Count == 0 ? NoContent() : Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AgentDetailDto>> GetById(string id)
        {
            var result = await _agentService.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("{id}/properties")]
        public async Task<IActionResult> GetAgentProperty(string id)
        {
            var result = await _propertyService.GetByAgentAsync(id, onlyAvailable: true);
            return result.Count == 0 ? NoContent() : Ok(result);
        }

        // Parte administrativa: activar e inactivar agente solo para Administrador 
        [HttpPatch("changestatus")]
        [Authorize(nameof(Roles.Administrador))]
        public async Task<IActionResult> ChangeStatus([FromQuery] string agentId, [FromBody] bool isActive)
        {
            var success = await _agentService.ChangeStatusAsync(agentId, isActive);
            return success ? NoContent() : NotFound();
        }
    }
}