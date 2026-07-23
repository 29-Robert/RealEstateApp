using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class AgentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyService _propertyService;
        private readonly IAccountService _accountService;

        public AgentController(
            UserManager<ApplicationUser> userManager,
            IPropertyService propertyService,
            IAccountService accountService)
        {
            _userManager = userManager;
            _propertyService = propertyService;
            _accountService = accountService;
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

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteAgent(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "El ID del agente no es válido.";
                return RedirectToAction("Index");
            }

            var response = await _accountService.DeleteAgentAsync(id);

            if (response.HasError)
            {
                TempData["ErrorMessage"] = response.ErrorDescription;
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "El agente y todo su ecosistema fueron eliminados exitosamente.";
            return RedirectToAction("Index");
        }
    }
}
