using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IAccountService _accountService;

        public AdminController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _accountService.GetAdminDashboardStatsAsync();
            return View(stats);
        }

        public async Task<IActionResult> Agents()
        {
            var agents = await _accountService.GetAgentsListAsync();
            return View(agents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAgentStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "El ID del agente no es válido.";
                return RedirectToAction(nameof(Agents));
            }

            await _accountService.ToggleAgentStatusAsync(id);
            TempData["SuccessMessage"] = "Estado del agente actualizado exitosamente.";
            return RedirectToAction(nameof(Agents));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAgentConfirm(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "El ID del agente no es válido.";
                return RedirectToAction(nameof(Agents));
            }

            var agent = await _accountService.GetUserByIdAsync(id);
            if (agent == null)
            {
                TempData["ErrorMessage"] = "El agente no existe.";
                return RedirectToAction(nameof(Agents));
            }

            var agentsList = await _accountService.GetAgentsListAsync();
            var agentInfo = agentsList.Find(a => a.Id == id);
            ViewBag.PropertyCount = agentInfo?.PropertyCount ?? 0;

            return View(agent);
        }
    }
}
