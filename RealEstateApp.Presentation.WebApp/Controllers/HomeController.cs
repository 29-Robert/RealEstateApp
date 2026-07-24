using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Message;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly IFavoriteService _favoriteService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMessageService _messageService;

        public HomeController(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            IFavoriteService favoriteService,
            UserManager<ApplicationUser> userManager,
            IMessageService messageService)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _favoriteService = favoriteService;
            _userManager = userManager;
            _messageService = messageService;
        }

        public async Task<IActionResult> Index(
            int? propertyTypeId,
            decimal? minPrice,
            decimal? maxPrice,
            int? rooms,
            int? bathrooms,
            int? code)
        {
            // Búsqueda por código exacto
            if (code.HasValue)
            {
                var found = await _propertyService.GetByCodeAsync(code.Value);
                if (found != null)
                {
                    return RedirectToAction(nameof(Details), new { id = found.Id });
                }
                TempData["SearchError"] = $"No se encontró ninguna propiedad con el código {code.Value}.";
            }

            var properties = await _propertyService.GetFilteredAsync(
                propertyTypeId, minPrice, maxPrice, rooms, bathrooms);

            // =========================================================
            // NUEVO: Marcar las propiedades favoritas del cliente
            // =========================================================
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Cliente"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var favoriteIds = await _favoriteService.GetClientFavoritePropertyIdsAsync(user.Id);
                    foreach (var prop in properties)
                    {
                        prop.IsFavorite = favoriteIds.Contains(prop.Id);
                    }
                }
            }

            var propertyTypes = await _propertyTypeService.GetAllViewModel();

            ViewBag.PropertyTypes = propertyTypes;
            ViewBag.SelectedTypeId = propertyTypeId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Rooms = rooms;
            ViewBag.Bathrooms = bathrooms;
            ViewBag.Code = code;

            return View(properties);
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);
            if (property == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Marcar si es favorita y cargar el chat en la vista de detalle si es cliente
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Cliente"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var favoriteIds = await _favoriteService.GetClientFavoritePropertyIdsAsync(user.Id);
                    property.IsFavorite = favoriteIds.Contains(property.Id);

                    property.ChatMessages = await _messageService.GetConversationAsync(property.Id, user.Id);
                }
            }

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(SendMessageViewModel vm)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Cliente"))
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                TempData["ChatError"] = "Debe escribir un mensaje antes de enviarlo.";
                return RedirectToAction(nameof(Details), new { id = vm.PropertyId });
            }

            var property = await _propertyService.GetByIdAsync(vm.PropertyId);
            if (property == null || property.Status != "Disponible")
            {
                TempData["ChatError"] = "La propiedad no existe o no está disponible.";
                return RedirectToAction(nameof(Details), new { id = vm.PropertyId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            await _messageService.SendMessageAsync(vm, user.Id);
            return RedirectToAction(nameof(Details), new { id = vm.PropertyId });
        }
    }
}