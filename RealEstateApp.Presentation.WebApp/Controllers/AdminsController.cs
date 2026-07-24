using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Users;
using RealEstateApp.Infrastructure.Identity.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminsController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminsController(IAccountService accountService, UserManager<ApplicationUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Administrador");
            var currentUserId = GetCurrentUserId();
            ViewBag.CurrentUserId = currentUserId;
            return View(usersInRole);
        }

        public IActionResult Create()
        {
            return View(new RegisterViewModel { UserType = "Administrador" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var response = await _accountService.RegisterAdminOrDeveloperAsync(vm, "Administrador");

            if (response.HasError)
            {
                ModelState.AddModelError(string.Empty, response.ErrorDescription ?? "Error al registrar el administrador.");
                return View(vm);
            }

            TempData["SuccessMessage"] = "Administrador creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return RedirectToAction(nameof(Index));

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Username = user.UserName!,
                PhoneNumber = user.PhoneNumber!
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var response = await _accountService.UpdateAdminOrDeveloperAsync(vm);

            if (response.HasError)
            {
                ModelState.AddModelError(string.Empty, response.ErrorDescription ?? "Error al actualizar.");
                return View(vm);
            }

            TempData["SuccessMessage"] = "Administrador actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var currentUserId = GetCurrentUserId();
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "No puedes inactivar tu propia cuenta de administrador.";
                return RedirectToAction(nameof(Index));
            }

            await _accountService.ToggleUserStatusAsync(id);
            TempData["SuccessMessage"] = "Estado de la cuenta de administrador actualizado.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

            var currentUserId = GetCurrentUserId();
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "No puedes eliminar tu propia cuenta de administrador.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return RedirectToAction(nameof(Index));

            return View(user);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(string id)
        {
            var currentUserId = GetCurrentUserId();
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "No puedes eliminar tu propia cuenta de administrador.";
                return RedirectToAction(nameof(Index));
            }

            var response = await _accountService.DeleteAdminOrDeveloperAsync(id);
            if (response.HasError)
            {
                TempData["ErrorMessage"] = response.ErrorDescription;
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Administrador eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
