using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Users;

namespace RealEstateApp.Presentation.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] LoginViewModel request)
        {
            var response = await _accountService.AuthenticateAsync(request);
            if (response.HasError)
            {
                return BadRequest(response.ErrorDescription);
            }
            return Ok(response);
        }

        // =================================================================
        // ENDPOINT PARA REGISTRAR ADMINISTRADORES
        // =================================================================
        [Authorize(Roles = "Administrador")]
        [HttpPost("register-admin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegisterViewModel request)
        {
            var response = await _accountService.RegisterAdminOrDeveloperAsync(request, "Admin");
            if (response.HasError)
            {
                return BadRequest(response.ErrorDescription);
            }
            return Created(string.Empty, new { Message = "Administrador registrado exitosamente." });
        }

        // =================================================================
        // ENDPOINT PARA REGISTRAR DESARROLLADORES
        // =================================================================
        [Authorize(Roles = "Admin")]
        [HttpPost("register-developer")]
        public async Task<IActionResult> RegisterDeveloperAsync([FromBody] RegisterViewModel request)
        {
            var response = await _accountService.RegisterAdminOrDeveloperAsync(request, "Developer");
            if (response.HasError)
            {
                return BadRequest(response.ErrorDescription);
            }
            return Created(string.Empty, new { Message = "Desarrollador registrado exitosamente." });
        }
    }
}