using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Enums;
using System.Security.Claims;
using RealEstateApp.Application.ViewModels;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = nameof(Roles.Agente))]
    public class OffersController : Controller
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        private string AgentId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var offers = await _offerService.GetByAgentAsync(AgentId);
            return View(offers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            await _offerService.AcceptOfferAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            await _offerService.RejectOfferAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}