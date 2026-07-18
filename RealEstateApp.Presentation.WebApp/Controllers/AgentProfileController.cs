using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Infrastructure.Identity.Entities;
using System.Security.Claims;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Agente")]
    public class AgentProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AgentProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return RedirectToAction("Login", "Account");

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Index(
            string firstName,
            string lastName,
            string phoneNumber,
            string? cardId,
            IFormFile? photo)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return RedirectToAction("Login", "Account");

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.CardId = cardId;

            if (photo != null && photo.Length > 0)
            {
                using var ms = new MemoryStream();
                await photo.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                var ext = Path.GetExtension(photo.FileName).TrimStart('.');
                user.PhotoPath = $"data:image/{ext};base64,{base64}";
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al actualizar el perfil.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
