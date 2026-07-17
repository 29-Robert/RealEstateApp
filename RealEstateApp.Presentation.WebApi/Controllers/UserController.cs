using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels;
using System.Threading.Tasks;

namespace RealEstateApp.Presentation.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public UserController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] Application.ViewModels.Users.RegisterViewModel request)
        {
            var origin = Request.Headers["origin"].ToString();
            if (string.IsNullOrEmpty(origin))
            {
                origin = $"{Request.Scheme}://{Request.Host}";
            }

            var response = await _accountService.RegisterUserAsync(request, origin);

            if (response.HasError)
            {
                return BadRequest(new { Error = response.ErrorDescription });
            }

            return Ok(response);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { Error = "El identificador de usuario (userId) y el token de verificación son requeridos." });
            }

            var result = await _accountService.ConfirmEmailAsync(userId, token);

            if (result.Contains("error") || result.Contains("No se encontró"))
            {
                return BadRequest(new { Message = result });
            }

            return Ok(new { Message = result });
        }
    }
}