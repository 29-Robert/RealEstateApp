using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Users;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectByRole();
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var response = await _accountService.AuthenticateAsync(vm);

            if (response.HasError)
            {
                ModelState.AddModelError(string.Empty, response.ErrorDescription ?? "Error al iniciar sesión.");
                return View(vm);
            }

            // --- BLOQUEO DE ACCESO PARA EL ROL DESARROLLADOR ---
            if (response.Roles != null && response.Roles.Contains("Desarrollador"))
            {
                await _accountService.SignOutAsync();

                ModelState.AddModelError(string.Empty, "Acceso denegado. Los usuarios con rol Desarrollador solo pueden acceder a la Web API.");
                return View(vm);
            }

            // Guardar datos de sesión
            HttpContext.Session.SetString("UserId", response.Id);
            HttpContext.Session.SetString("UserName", response.UserName);
            HttpContext.Session.SetString("UserRoles", string.Join(",", response.Roles!));

            return RedirectByRole();
        }

        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectByRole();
            }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var origin = Request.Headers["Origin"].ToString();
            var response = await _accountService.RegisterUserAsync(vm, origin);

            if (response.HasError)
            {
                ModelState.AddModelError("Summary", response.ErrorDescription ?? "Error al registrarse.");
                return View(vm);
            }

            // --- MENSAJES DE ÉXITO CONDICIONADOS POR ROL ---
            if (vm.UserType == "Agente")
            {
                TempData["SuccessMessage"] = "Cuenta de agente creada exitosamente. Su cuenta requiere de la aprobación y activación manual por parte de un administrador para poder iniciar sesión.";
            }
            else // Cliente
            {
                TempData["SuccessMessage"] = "Cuenta creada exitosamente. Por favor, revisa tu correo electrónico para activar tu cuenta antes de iniciar sesión.";
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _accountService.ConfirmEmailAsync(userId, token);
            TempData["ActivationResult"] = result;
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        private IActionResult RedirectByRole()
        {
            if (User.IsInRole("Administrador"))
                return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Agente"))
                return RedirectToAction("Index", "Property");
            if (User.IsInRole("Cliente"))
                return RedirectToAction("ClientHome", "Home");

            return RedirectToAction("Index", "Home");
        }
    }
}