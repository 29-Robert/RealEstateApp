using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Message;
using RealEstateApp.Application.ViewModels.Offer;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Agente,Administrador")]
    public class AgentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyService _propertyService;
        private readonly IAccountService _accountService;
        private readonly IMessageService _messageService;
        private readonly IOfferService _offerService;

        public AgentController(
            UserManager<ApplicationUser> userManager,
            IPropertyService propertyService,
            IAccountService accountService,
            IMessageService messageService,
            IOfferService offerService)
        {
            _userManager = userManager;
            _propertyService = propertyService;
            _accountService = accountService;
            _messageService = messageService;
            _offerService = offerService;
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        private string GetCurrentUserId() =>
            _userManager.GetUserId(User) ?? string.Empty;

        [Authorize(Roles = "Agente")]
        public async Task<IActionResult> PropertyDetails(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);
            var agentId = GetCurrentUserId();

            if (property == null || property.AgentId != agentId)
            {
                return RedirectToAction("Index", "Property");
            }

            var allAgentMessages = await _messageService.GetAgentMessagesAsync(agentId);
            var propertyMessages = allAgentMessages
                .Where(m => m.PropertyId == id)
                .ToList();

            var clientConversations = propertyMessages
                .GroupBy(m => m.ClientId)
                .Select(g => new AgentConversationViewModel
                {
                    ClientId = g.Key,
                    ClientName = g.First().ClientName ?? "Cliente",
                    LastMessage = g.OrderByDescending(m => m.SentAt).First().Content,
                    LastMessageDate = g.OrderByDescending(m => m.SentAt).First().SentAt
                })
                .OrderByDescending(c => c.LastMessageDate)
                .ToList();

            ViewBag.Conversations = clientConversations;

            // Consultar las ofertas de esta propiedad agrupadas por cliente
            var propertyOffers = await _offerService.GetOffersByPropertyAsync(id);
            var clientOffersGrouped = propertyOffers
                .GroupBy(o => o.ClientId)
                .Select(g => new AgentPropertyOfferGroupViewModel
                {
                    ClientId = g.Key,
                    ClientName = g.First().ClientFullName ?? "Cliente",
                    OfferCount = g.Count(),
                    LastOfferAmount = g.OrderByDescending(o => o.OfferDate).First().Amount,
                    LastOfferStatus = g.OrderByDescending(o => o.OfferDate).First().Status,
                    LastOfferDate = g.OrderByDescending(o => o.OfferDate).First().OfferDate
                })
                .OrderByDescending(g => g.LastOfferDate)
                .ToList();

            ViewBag.PropertyOffers = clientOffersGrouped;

            return View(property);
        }

        [Authorize(Roles = "Agente")]
        public async Task<IActionResult> Conversation(int propertyId, string clientId)
        {
            var property = await _propertyService.GetByIdAsync(propertyId);
            var agentId = GetCurrentUserId();

            if (property == null || property.AgentId != agentId)
            {
                return RedirectToAction("Index", "Property");
            }

            var messages = await _messageService.GetConversationAsync(propertyId, clientId);
            var client = await _accountService.GetUserByIdAsync(clientId);

            ViewBag.Property = property;
            ViewBag.ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "Cliente";
            ViewBag.ClientId = clientId;

            return View(messages);
        }

        [HttpPost]
        [Authorize(Roles = "Agente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyMessage(ReplyMessageViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["ChatError"] = "Debe escribir un mensaje antes de enviarlo.";
                return RedirectToAction(nameof(Conversation), new { propertyId = vm.PropertyId, clientId = vm.ClientId });
            }

            var agentId = GetCurrentUserId();
            await _messageService.ReplyMessageAsync(vm, agentId);
            return RedirectToAction(nameof(Conversation), new { propertyId = vm.PropertyId, clientId = vm.ClientId });
        }

        [Authorize(Roles = "Agente")]
        public async Task<IActionResult> ClientOffers(int propertyId, string clientId)
        {
            var property = await _propertyService.GetByIdAsync(propertyId);
            var agentId = GetCurrentUserId();

            if (property == null || property.AgentId != agentId)
            {
                return RedirectToAction("Index", "Property");
            }

            var offers = await _offerService.GetClientOffersAsync(propertyId, clientId);
            var client = await _accountService.GetUserByIdAsync(clientId);

            ViewBag.Property = property;
            ViewBag.ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "Cliente";
            ViewBag.ClientId = clientId;

            return View(offers);
        }

        [HttpPost]
        [Authorize(Roles = "Agente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOffer(int offerId, int propertyId)
        {
            var property = await _propertyService.GetByIdAsync(propertyId);
            var agentId = GetCurrentUserId();

            if (property == null || property.AgentId != agentId)
            {
                return RedirectToAction("Index", "Property");
            }

            await _offerService.AcceptOfferAsync(offerId, agentId);
            TempData["SuccessMessage"] = "La oferta fue aceptada correctamente y la propiedad fue marcada como vendida.";
            return RedirectToAction(nameof(PropertyDetails), new { id = propertyId });
        }

        [HttpPost]
        [Authorize(Roles = "Agente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOffer(int offerId, int propertyId)
        {
            var property = await _propertyService.GetByIdAsync(propertyId);
            var agentId = GetCurrentUserId();

            if (property == null || property.AgentId != agentId)
            {
                return RedirectToAction("Index", "Property");
            }

            await _offerService.RejectOfferAsync(offerId, agentId);
            TempData["SuccessMessage"] = "La oferta fue rechazada correctamente.";
            return RedirectToAction(nameof(PropertyDetails), new { id = propertyId });
        }
    }
}
