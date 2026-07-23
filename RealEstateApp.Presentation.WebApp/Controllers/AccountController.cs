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

            // Guardar datos de sesión
            HttpContext.Session.SetString("UserId", response.Id);
            HttpContext.Session.SetString("UserName", response.UserName);
            HttpContext.Session.SetString("UserRoles", string.Join(",", response.Roles));

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

            TempData["SuccessMessage"] = "Cuenta creada exitosamente. Por favor, revisa tu correo electrónico para confirmar tu cuenta antes de iniciar sesión.";
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

        public IActionResult AccesDenied()
        {
            return View();
        }

        private IActionResult RedirectByRole()
        {
            if (User.IsInRole("Administrador"))
                return RedirectToAction("Index", "Home");
            if (User.IsInRole("Agente"))
                return RedirectToAction("Index", "Property");
            if (User.IsInRole("Cliente"))
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Home");
        }
    }
}
