using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Services;
using RealEstateApp.Application.ViewModels.Message; 
using RealEstateApp.Domain.Enums;
using System.Security.Claims;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = nameof(Roles.Agente))]
    public class InboxController : Controller
    {
        private readonly IMessageService _messageService; 

        public InboxController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        private string AgentId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            // CUsar GetAgentMessagesAsync tal como está en IMessageService
            var conversations = await _messageService.GetAgentMessagesAsync(AgentId);
            return View(conversations);
        }

        public async Task<IActionResult> Conversation(int propertyId, string clientId)
        {
            var messages = await _messageService.GetConversationAsync(propertyId, clientId);
            ViewBag.PropertyId = propertyId;
            ViewBag.ClientId = clientId;
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int propertyId, string clientId, string message)
        {
            var vm = new SendMessageViewModel
            {
                PropertyId = propertyId,
                AgentId = AgentId,
                Content = message  
            };

            await _messageService.SendMessageAsync(vm, clientId);

            return RedirectToAction(nameof(Conversation), new { propertyId, clientId });
        }
    }
    
}