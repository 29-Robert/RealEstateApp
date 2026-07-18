using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    /// <summary>
    /// Catálogo público de Agentes (visible sin autenticación)
    /// </summary>
    public class AgentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyService _propertyService;

        public AgentController(
            UserManager<ApplicationUser> userManager,
            IPropertyService propertyService)
        {
            _userManager = userManager;
            _propertyService = propertyService;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var agents = await _userManager.GetUsersInRoleAsync("Agente");

            var activeAgents = agents
                .Where(a => a.IsActive)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                activeAgents = activeAgents.Where(a =>
                    a.FirstName.ToLower().Contains(lower) ||
                    a.LastName.ToLower().Contains(lower));
            }

            // Ordenar alfabéticamente por apellido
            activeAgents = activeAgents.OrderBy(a => a.LastName).ThenBy(a => a.FirstName);

            var agentList = new List<(ApplicationUser Agent, int PropertyCount)>();
            foreach (var agent in activeAgents)
            {
                var props = await _propertyService.GetByAgentAsync(agent.Id, true);
                agentList.Add((agent, props.Count));
            }

            ViewBag.Search = search;
            return View(agentList);
        }

        public async Task<IActionResult> Properties(string agentId)
        {
            var agent = await _userManager.FindByIdAsync(agentId);
            if (agent == null || !agent.IsActive)
            {
                return RedirectToAction(nameof(Index));
            }

            var properties = await _propertyService.GetByAgentAsync(agentId, true);

            ViewBag.AgentName = $"{agent.FirstName} {agent.LastName}";
            ViewBag.AgentPhoto = agent.PhotoPath;
            ViewBag.AgentPhone = agent.PhoneNumber;
            ViewBag.AgentEmail = agent.Email;
            return View(properties);
        }
    }
}
