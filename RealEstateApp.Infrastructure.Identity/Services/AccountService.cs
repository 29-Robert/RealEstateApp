using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.DTOs.Email;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Users;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Infrastructure.Identity.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWTSettings _jwtSettings;
        private readonly IEmailService _emailService;


        public AccountService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IOptions<JWTSettings> jwtSettings,
            IEmailService emailService) 
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _emailService = emailService;
        }

        // 1. PROCESO DE LOGIN
        public async Task<AuthenticationResponse> AuthenticateAsync(LoginViewModel loginVm)
        {
            var user = await _userManager.FindByEmailAsync(loginVm.Email);
            if (user == null)
            {
                return new AuthenticationResponse { HasError = true, ErrorDescription = "Credenciales incorrectas." };
            }

            // Cambiado a false fíjamente por el requerimiento de quitar el "Recordarme"
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginVm.Password, false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return new AuthenticationResponse { HasError = true, ErrorDescription = "Credenciales incorrectas." };
            }

            if (!user.EmailConfirmed)
            {
                return new AuthenticationResponse { HasError = true, ErrorDescription = "Debes activar tu cuenta por correo antes de iniciar sesión." };
            }

            // Buscamos los roles asignados al usuario en Identity
            var roles = await _userManager.GetRolesAsync(user);

            var response = new AuthenticationResponse
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

            // Definimos los Claims (los datos integrados cifrados dentro del token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            // Agregamos cada rol correspondiente del usuario a los claims
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

            // Asignamos el string generado a la propiedad de la respuesta
            response.JWToken = tokenHandler.WriteToken(token);

            return response;
        }
        
        // 2. PROCESO DE REGISTRO DIFERENCIADO (CLIENTES VS AGENTES)
        public async Task<RegisterResponse> RegisterUserAsync(RegisterViewModel registerVm, string origin)
        {
            var response = new RegisterResponse();

            // Validar que el rol solicitado sea permitido de forma pública
            if (registerVm.UserType != Roles.Cliente.ToString() && registerVm.UserType != Roles.Agente.ToString()) 
            {
                response.HasError = true;
                response.ErrorDescription = "Acción no permitida. Solo se permite el registro público de Clientes y Agentes.";
                return response;
            }

            // Validar si el nombre de usuario o correo ya existen
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

            // Crear la instancia de nuestro usuario personalizado
            var newUser = new ApplicationUser
            {
                FirstName = registerVm.FirstName,
                LastName = registerVm.LastName,
                Email = registerVm.Email,
                UserName = registerVm.Username,
                PhoneNumber = registerVm.PhoneNumber,
                // Si es Cliente empieza inactivo (hasta confirmar correo). Si es Agente empieza inactivo (hasta aprobación manual).
                IsActive = false
            };

            // Intentar registrar el usuario en la base de datos
            var result = await _userManager.CreateAsync(newUser, registerVm.Password);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.ErrorDescription = result.Errors.FirstOrDefault()?.Description ?? "Ocurrió un error al registrar el usuario.";
                return response;
            }

            // Asignar el Rol correspondiente en la tabla intermedia de Identity
            await _userManager.AddToRoleAsync(newUser, registerVm.UserType);

            // FLUJO DIFERENCIADO: Envío de correo electrónico exclusivamente a Clientes
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

            // Decodificar el token web seguro recibido por URL
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            // Confirmar el token con Identity
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                user.IsActive = true; // Activamos al usuario en el sistema
                await _userManager.UpdateAsync(user);
                return "Cuenta activada exitosamente. Ya puedes iniciar sesión.";
            }

            return "Ocurrió un error intentando validar el token de activación.";
        }

        // MÉTODOS AUXILIARES: Generación segura de la URL de activación
        private async Task<string> GenerateVerificationUrl(ApplicationUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var route = "/Account/ConfirmEmail";
            var url = $"{origin}{route}?userId={user.Id}&token={encodedToken}";
            return url;
        }

        public async Task<RegisterResponse> RegisterAdminOrDeveloperAsync(RegisterViewModel request, string role)
        {
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null)
            {
                return new RegisterResponse { HasError = true, ErrorDescription = $"El correo '{request.Email}' ya está registrado." };
            }

            var userWithSameUsername = await _userManager.FindByNameAsync(request.Username);
            if (userWithSameUsername != null)
            {
                return new RegisterResponse { HasError = true, ErrorDescription = $"El nombre de usuario '{request.Username}' ya está en uso." };
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
                return new RegisterResponse { HasError = true, ErrorDescription = errors.ToString() };
            }

            await _userManager.AddToRoleAsync(user, role);

            return new RegisterResponse { HasError = false };
        }
    }
}