using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using System.Security.Claims;

[Authorize(Roles = "Agente")]
public class ChatController : Controller
{
    private readonly IMessageRepository _messageRepository;
    private readonly IPropertyRepository _propertyRepository;

    public ChatController(IMessageRepository messageRepository, IPropertyRepository propertyRepository)
    {
        _messageRepository = messageRepository;
        _propertyRepository = propertyRepository;
    }

    // Vista para que el agente vea el chat con un cliente sobre una propiedad específica
    public async Task<IActionResult> Conversation(string clientId, int propertyId)
    {
        var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier); // ID del agente logueado

        // Obtenemos el historial usando los campos exactos de tu entidad Message
        var messages = await _messageRepository.GetChatHistoryAsync(clientId, agentId, propertyId);

        // Puedes pasar los datos a la vista mediante un ViewModel o directamente
        ViewBag.ClientId = clientId;
        ViewBag.PropertyId = propertyId;

        return View(messages);
    }

    // Acción para que el agente responda y guarde el mensaje
    [HttpPost]
    public async Task<IActionResult> SendMessage(string clientId, int propertyId, string content)
    {
        var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var message = new Message
        {
            ClientId = clientId,
            AgentId = agentId,
            PropertyId = propertyId,
            Content = content,
            SentAt = DateTime.Now,
            IsFromClient = false // Como lo manda el agente, es falso
        };

        await _messageRepository.AddAsync(message);

        // Redirige de vuelta a la conversación
        return RedirectToAction(nameof(Conversation), new { clientId, propertyId });
    }
}