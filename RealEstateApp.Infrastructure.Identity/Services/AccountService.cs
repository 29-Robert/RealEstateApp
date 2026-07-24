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
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
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
        private readonly IGenericRepository<PropertyImage> _propertyImageRepository;
        private readonly IGenericRepository<Offer> _offerRepository;
        private readonly IGenericRepository<Favorite> _favoriteRepository;
        private readonly IGenericRepository<ChatMessage> _chatRepository;
        private readonly ILogger<AccountService> _logger;


        public AccountService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IOptions<JWTSettings> jwtSettings,
            IEmailService emailService,
            IGenericRepository<Property> propertyRepository,
            IGenericRepository<PropertyImage> propertyImageRepository,
            IGenericRepository<Offer> offerRepository,
            IGenericRepository<Favorite> favoriteRepository,
            IGenericRepository<ChatMessage> chatRepository,
            ILogger<AccountService> logger) 
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _offerRepository = offerRepository;
            _favoriteRepository = favoriteRepository;
            _chatRepository = chatRepository;
            _logger = logger;
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
                await _signInManager.SignOutAsync();
                return new AuthenticationResponseDto { HasError = true, ErrorDescription = "Debes activar tu cuenta por correo antes de iniciar sesión." };
            }

            // Buscamos los roles asignados al usuario en Identity
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Agente") && !user.IsActive)
            {
                await _signInManager.SignOutAsync();
                return new AuthenticationResponseDto
                {
                    HasError = true,
                    ErrorDescription = "Su cuenta de agente ha sido inactivada por el administrador. No puede iniciar sesión."
                };
            }

            if (roles.Contains("Cliente") && !user.IsActive)
            {
                await _signInManager.SignOutAsync();
                return new AuthenticationResponseDto
                {
                    HasError = true,
                    ErrorDescription = "Su cuenta de cliente ha sido inactivada. No puede iniciar sesión."
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

            string photoPath = "/images/users/default-profile.png";

            if (registerVm.Photo != null && registerVm.Photo.Length > 0)
            {
                try
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(registerVm.Photo.FileName);
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await registerVm.Photo.CopyToAsync(stream);
                    }

                    photoPath = $"/images/users/{fileName}";
                }
                catch (Exception)
                {
                    // Si falla el guardado físico, se queda con la imagen por defecto
                }
            }

            var newUser = new ApplicationUser
            {
                FirstName = registerVm.FirstName,
                LastName = registerVm.LastName,
                Email = registerVm.Email,
                UserName = registerVm.Username,
                PhoneNumber = registerVm.PhoneNumber,
                CardId = registerVm.CardId,
                PhotoPath = photoPath,
                IsActive = false,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(newUser, registerVm.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new RegisterResponseDto { HasError = true, ErrorDescription = errors };
            }

            await _userManager.AddToRoleAsync(newUser, registerVm.UserType);

            if (registerVm.UserType == Roles.Cliente.ToString())
            {
                var verificationUrl = await GenerateVerificationUrl(newUser, origin);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var body = $"<h1>¡Hola {newUser.FirstName}!</h1><p>Gracias por registrarte. Para activar tu cuenta de Cliente, por favor haz clic en el siguiente enlace:</p><a href='{verificationUrl}'>Activar Mi Cuenta</a>";
                        await _emailService.SendAsync(newUser.Email ?? "", "Activación de Cuenta - RealEstateApp", body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"---> ERROR EN SEGUNDO PLANO ENVIANDO CORREO: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"---> DETALLE: {ex.InnerException.Message}");
                        }
                        _logger.LogError(ex, "Fallo al enviar correo de confirmación en segundo plano.");
                    }
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
            var url = $"{origin}{route}?userId={System.Net.WebUtility.UrlEncode(user.Id)}&token={System.Net.WebUtility.UrlEncode(encodedToken)}";
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

        public async Task<UserResponseDto?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                UserName = user.UserName!
            };
        }

        public async Task ToggleAgentStatusAsync(string agentId)
        {
            var user = await _userManager.FindByIdAsync(agentId);

            if (user == null)
            {
                throw new KeyNotFoundException("El agente especificado no existe en el sistema.");
            }

            user.IsActive = !user.IsActive;
            if (user.IsActive)
            {
                user.EmailConfirmed = true;
            }

            await _userManager.UpdateAsync(user);
        }

        public async Task<List<AgentResponseDto>> GetAgentsListAsync()
        {
            var agentsInRole = await _userManager.GetUsersInRoleAsync("Agente");
            var agentList = new List<AgentResponseDto>();

            var properties = await _propertyRepository.GetAllAsync();

            foreach (var user in agentsInRole)
            {
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

        public async Task<RegisterResponseDto> DeleteAgentAsync(string agentId)
        {
            var response = new RegisterResponseDto();

            var agent = await _userManager.FindByIdAsync(agentId);
            if (agent == null)
            {
                response.HasError = true;
                response.ErrorDescription = "El agente especificado no existe.";
                return response;
            }

            var allProperties = await _propertyRepository.GetAllAsync();
            var agentProperties = allProperties.Where(p => p.AgentId == agentId).ToList();

            try
            {
                // 1. Descargamos los listados transaccionales dependientes en memoria
                var allImages = await _propertyImageRepository.GetAllAsync();
                var allOffers = await _offerRepository.GetAllAsync();
                var allFavorites = await _favoriteRepository.GetAllAsync();
                var allChats = await _chatRepository.GetAllAsync(); // <-- Descarga de chats añadida

                foreach (var property in agentProperties)
                {
                    // A. Limpiar las imágenes de la propiedad
                    var propertyImages = allImages.Where(i => i.PropertyId == property.Id).ToList();
                    foreach (var img in propertyImages)
                    {
                        await _propertyImageRepository.DeleteAsync(img);
                    }

                    // B. Limpiar las ofertas recibidas por la propiedad
                    var propertyOffers = allOffers.Where(o => o.PropertyId == property.Id).ToList();
                    foreach (var offer in propertyOffers)
                    {
                        await _offerRepository.DeleteAsync(offer);
                    }

                    // C. Limpiar los favoritos asociados a la propiedad
                    var propertyFavorites = allFavorites.Where(f => f.PropertyId == property.Id).ToList();
                    foreach (var fav in propertyFavorites)
                    {
                        await _favoriteRepository.DeleteAsync(fav);
                    }

                    // D. Borrar la entidad Propiedad. 
                    // EF Core limpiará automáticamente la relación con las Improvements aquí.
                    await _propertyRepository.DeleteAsync(property);
                }

                // E. Limpiar los chats directos del agente (Emisor o Receptor)
                var agentChats = allChats.Where(c => c.SenderId == agentId || c.ReceiverId == agentId).ToList();
                foreach (var chat in agentChats)
                {
                    await _chatRepository.DeleteAsync(chat);
                }

                // 2. Borrar definitivamente al agente de la base de datos de Identity
                var result = await _userManager.DeleteAsync(agent);
                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.ErrorDescription = "No se pudo eliminar el usuario de las credenciales de Identity.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.ErrorDescription = $"Error crítico durante el borrado en cascada: {ex.Message}";
                return response;
            }

            return response;
        }

    }
}