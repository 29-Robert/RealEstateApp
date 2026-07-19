using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.DTOs.Email;
using RealEstateApp.Application.Interfaces.Repositories; 
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Users;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Infrastructure.Identity.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RealEstateApp.Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWTSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly IGenericRepository<Property> _propertyRepository; 


        public AccountService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IOptions<JWTSettings> jwtSettings,
            IEmailService emailService,
            IGenericRepository<Property> propertyRepository) 
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
            _propertyRepository = propertyRepository; 
        }

        // 1. PROCESO DE LOGIN
        public async Task<AuthenticationResponseDto> AuthenticateAsync(LoginViewModel loginVm)
        {
            var user = await _userManager.FindByEmailAsync(loginVm.Email);
            if (user == null)
            {
                return new AuthenticationResponseDto { HasError = true, ErrorDescription = "Credenciales incorrectas." };
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginVm.Password, false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return new AuthenticationResponseDto { HasError = true, ErrorDescription = "Credenciales incorrectas." };
            }

            if (!user.EmailConfirmed)
            {
                return new AuthenticationResponseDto { HasError = true, ErrorDescription = "Debes activar tu cuenta por correo antes de iniciar sesión." };
            }

            // Buscamos los roles asignados al usuario en Identity
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Agente") && !user.IsActive)
            {
                return new AuthenticationResponseDto
                {
                    HasError = true,
                    ErrorDescription = "Su cuenta de agente ha sido inactivada por el administrador. No puede iniciar sesión."
                };
            }

            var response = new AuthenticationResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                Roles = new List<string>(roles),
                HasError = false
            };

            // =======================================================
            // GENERACIÓN DEL TOKEN JWT PARA LA API REST
            // =======================================================
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            response.JWToken = tokenHandler.WriteToken(token);

            return response;
        }

        // 2. PROCESO DE REGISTRO DIFERENCIADO (CLIENTES VS AGENTES)
        public async Task<RegisterResponseDto> RegisterUserAsync(RegisterViewModel registerVm, string origin)
        {
            var response = new RegisterResponseDto();

            if (registerVm.UserType != Roles.Cliente.ToString() && registerVm.UserType != Roles.Agente.ToString())
            {
                response.HasError = true;
                response.ErrorDescription = "Acción no permitida. Solo se permite el registro público de Clientes y Agentes.";
                return response;
            }

            var userByUsername = await _userManager.FindByNameAsync(registerVm.Username);
            if (userByUsername != null)
            {
                response.HasError = true;
                response.ErrorDescription = $"El nombre de usuario '{registerVm.Username}' ya está en uso.";
                return response;
            }

            var userByEmail = await _userManager.FindByEmailAsync(registerVm.Email);
            if (userByEmail != null)
            {
                response.HasError = true;
                response.ErrorDescription = $"El correo '{registerVm.Email}' ya está registrado.";
                return response;
            }

            var newUser = new ApplicationUser
            {
                FirstName = registerVm.FirstName,
                LastName = registerVm.LastName,
                Email = registerVm.Email,
                UserName = registerVm.Username,
                PhoneNumber = registerVm.PhoneNumber,
                IsActive = false
            };

            var result = await _userManager.CreateAsync(newUser, registerVm.Password);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.ErrorDescription = result.Errors.FirstOrDefault()?.Description ?? "Ocurrió un error al registrar el usuario.";
                return response;
            }

            await _userManager.AddToRoleAsync(newUser, registerVm.UserType);

            if (registerVm.UserType == Roles.Cliente.ToString())
            {
                var verificationUrl = await GenerateVerificationUrl(newUser, origin);

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = newUser.Email,
                    Subject = "Activación de Cuenta - RealEstateApp",
                    HtmlBody = $"<h1>¡Hola {newUser.FirstName}!</h1><p>Gracias por registrarte. Para activar tu cuenta de Cliente, por favor haz clic en el siguiente enlace:</p><a href='{verificationUrl}'>Activar Mi Cuenta</a>"
                });
            }

            return response;
        }

        // 3. PROCESO DE CERRAR SESIÓN
        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // 4. CONFIRMACIÓN DE EMAIL (PULSAR ENLACE DEL CORREO)
        public async Task<string> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "No se encontró el usuario para activar la cuenta.";
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
                return "Cuenta activada exitosamente. Ya puedes iniciar sesión.";
            }

            return "Ocurrió un error intentando validar el token de activación.";
        }

        private async Task<string> GenerateVerificationUrl(ApplicationUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var route = "/Account/ConfirmEmail";
            var url = $"{origin}{route}?userId={user.Id}&token={encodedToken}";
            return url;
        }

        public async Task<RegisterResponseDto> RegisterAdminOrDeveloperAsync(RegisterViewModel request, string role)
        {
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null)
            {
                return new RegisterResponseDto { HasError = true, ErrorDescription = $"El correo '{request.Email}' ya está registrado." };
            }

            var userWithSameUsername = await _userManager.FindByNameAsync(request.Username);
            if (userWithSameUsername != null)
            {
                return new RegisterResponseDto { HasError = true, ErrorDescription = $"El nombre de usuario '{request.Username}' ya está en uso." };
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                System.Text.StringBuilder errors = new();
                foreach (var error in result.Errors)
                {
                    errors.AppendLine(error.Description);
                }
                return new RegisterResponseDto { HasError = true, ErrorDescription = errors.ToString() };
            }

            await _userManager.AddToRoleAsync(user, role);

            return new RegisterResponseDto { HasError = false };
        }

        public async Task<List<UserResponseDto>> GetUsersInRoleAsync(string role)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var userList = new List<UserResponseDto>();

            foreach (var user in usersInRole)
            {
                userList.Add(new UserResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    UserName = user.UserName!
                });
            }

            return userList;
        }

        public async Task ToggleAgentStatusAsync(string agentId)
        {
            var user = await _userManager.FindByIdAsync(agentId);

            if (user == null)
            {
                throw new KeyNotFoundException("El agente especificado no existe en el sistema.");
            }

            user.IsActive = !user.IsActive;

            await _userManager.UpdateAsync(user);
        }

        public async Task<List<AgentResponseDto>> GetAgentsListAsync()
        {
            var agentsInRole = await _userManager.GetUsersInRoleAsync("Agente");
            var agentList = new List<AgentResponseDto>();

            var properties = await _propertyRepository.GetAllAsync();

            foreach (var user in agentsInRole)
            {
                // Contamos las propiedades asociadas a este Agente.
                // Nota: Si en 'Property.cs' el Dev B usó un nombre de campo diferente a 'AgentId' 
                // (como 'CreatedBy' o 'UsuarioId'), solo debes reemplazar 'AgentId' por ese nombre aquí abajo.
                int count = properties.Count(p => p.AgentId == user.Id);

                agentList.Add(new AgentResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber!,
                    IsActive = user.IsActive,
                    PropertyCount = count
                });
            }

            return agentList;
        }

        public async Task<AdminDashboardResponseDto> GetAdminDashboardStatsAsync()
        {
            // 1. Clasificación y conteo de usuarios desde ASP.NET Identity
            var agents = await _userManager.GetUsersInRoleAsync("Agente");
            var clients = await _userManager.GetUsersInRoleAsync("Cliente");
            var developers = await _userManager.GetUsersInRoleAsync("Desarrollador");
            var admins = await _userManager.GetUsersInRoleAsync("Administrador");

            // 2. Extracción de inventario comercial
            var properties = await _propertyRepository.GetAllAsync();

            // 3. Mapeo y cálculo consolidado del DTO
            return new AdminDashboardResponseDto
            {
                AgentesActivos = agents.Count(u => u.IsActive),
                AgentesInactivos = agents.Count(u => !u.IsActive),

                ClientesActivos = clients.Count(u => u.IsActive),
                ClientesInactivos = clients.Count(u => !u.IsActive),

                DesarroladoresActivos = developers.Count(u => u.IsActive),
                DesarroladoresInactivos = developers.Count(u => !u.IsActive),

                AdministradoresActivos = admins.Count(u => u.IsActive),
                AdministradoresInactivos = admins.Count(u => !u.IsActive),

                AvailableProperties = properties.Count(p => p.Status == "Disponible"),
                SoldProperties = properties.Count(p => p.Status == "Vendida")
            };
        }
    }
}