using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Agent,Client,Agente,Cliente")]
    public class ProfileController : Controller
    {
        private readonly IAccountService _accountService;

        public ProfileController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var profileVm = await _accountService.GetUserProfileByIdAsync(userId);
            if (profileVm == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(profileVm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SaveProfileViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.File != null && vm.File.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(vm.File.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("File", "Solo se permiten imágenes con formato .jpg, .jpeg o .png.");
                    return View(vm);
                }

                try
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.File.CopyToAsync(stream);
                    }

                    vm.ImagePath = $"/images/users/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error al guardar el archivo: {ex.Message}");
                    return View(vm);
                }
            }

            var response = await _accountService.UpdateProfileAsync(vm);

            if (response.HasError)
            {
                TempData["ErrorMessage"] = response.ErrorDescription ?? "Ocurrió un error al actualizar el perfil.";
                ModelState.AddModelError(string.Empty, response.ErrorDescription ?? "Error al actualizar el perfil.");
                return View(vm);
            }

            TempData["SuccessMessage"] = "Su perfil ha sido actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}