using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] LoginViewModel request)
        {
            var response = await _accountService.AuthenticateAsync(request);

            if (response.HasError)
            {
                return BadRequest(new { Error = response.ErrorDescription });
            }

            return Ok(response);
        }

        // =================================================================
        // ENDPOINT PARA REGISTRAR ADMINISTRADORES (PROTEGIDO)
        // =================================================================
        [HttpPost("register-admin")]
        [Authorize(Roles = "Administrador", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegisterViewModel request)
        {
            var response = await _accountService.RegisterAdminOrDeveloperAsync(request, "Administrador");

            if (response.HasError)
            {
                return BadRequest(new { Error = response.ErrorDescription });
            }

            return Created(string.Empty, new { Message = "Administrador registrado exitosamente." });
        }

        // =================================================================
        // ENDPOINT PARA REGISTRAR DESARROLLADORES (PROTEGIDO)
        // =================================================================
        [HttpPost("register-developer")]
        [Authorize(Roles = "Administrador", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RegisterDeveloperAsync([FromBody] RegisterViewModel request)
        {
            var response = await _accountService.RegisterAdminOrDeveloperAsync(request, "Desarrollador");

            if (response.HasError)
            {
                return BadRequest(new { Error = response.ErrorDescription });
            }

            return Created(string.Empty, new { Message = "Desarrollador registrado exitosamente." });
        }

        [HttpGet("/api/agents")]
        [Authorize(Roles = "Administrador", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAgentsAsync()
        {
            var agents = await _accountService.GetAgentsListAsync();
            return Ok(agents);
        }

        [HttpPatch("/api/agents/changestatus")]
        [Authorize(Roles = "Administrador", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeAgentStatusAsync([FromQuery] string agentId)
        {
            try
            {
                await _accountService.ToggleAgentStatusAsync(agentId);
                return Ok(new { Message = "El estado del agente ha sido modificado exitosamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // =================================================================
        // DASHBOARD / PANEL DE INDICADORES - TAREA 6 (PROTEGIDO)
        // =================================================================
        [HttpGet("/api/admin/dashboard")]
        [Authorize(Roles = "Administrador", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetDashboardStatsAsync()
        {
            var stats = await _accountService.GetAdminDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpDelete("/api/agents/{id}")]
        [Authorize(Roles = "Administrador", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteAgentAsync(string id)
        {
            var response = await _accountService.DeleteAgentAsync(id);

            if (response.HasError)
            {
                return BadRequest(new { Error = response.ErrorDescription });
            }

            return Ok(new { Message = "El agente y todo su ecosistema de datos han sido eliminados de forma exitosa." });
        }
    }
}