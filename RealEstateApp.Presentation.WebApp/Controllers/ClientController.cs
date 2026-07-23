using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClientController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientController(
            IFavoriteService favoriteService,
            UserManager<ApplicationUser> userManager)
        {
            _favoriteService = favoriteService;
            _userManager = userManager;
        }

        // Accion para marcar / desmarcar favorito
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int propertyId, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _favoriteService.ToggleFavoriteAsync(user.Id, propertyId);

            // Redirige a la página de origen para mantener al usuario donde estaba
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Pantalla exclusiva "Mis Favoritos"
        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var favoriteProperties = await _favoriteService.GetClientFavoritesAsync(user.Id);
            return View(favoriteProperties);
        }
        
    }
}