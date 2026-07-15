using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.DTOs.Email;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Users;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Infrastructure.Identity.Entities;
using System.Text;

namespace RealEstateApp.Infrastructure.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // 1. PROCESO DE LOGIN
        public async Task<AuthenticationResponse> AuthenticateAsync(LoginViewModel loginVm)
        {
            var response = new AuthenticationResponse();

            // Buscar al usuario por el correo electrónico introducido
            var user = await _userManager.FindByEmailAsync(loginVm.Email);
            if (user == null)
            {
                response.HasError = true;
                response.ErrorDescription = $"No existe ninguna cuenta registrada con el correo {loginVm.Email}.";
                return response;
            }

            // Validar si el usuario está activo (Regla de negocio solicitada)
            if (!user.IsActive)
            {
                response.HasError = true;
                response.ErrorDescription = "Su cuenta está inactiva. Si es Cliente, verifique su correo para activarla. Si es Agente, espere la aprobación de un Administrador.";
                return response;
            }

            // Intentar el inicio de sesión con la contraseña en texto plano
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginVm.Password, loginVm.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.ErrorDescription = "Contraseña incorrecta. Inténtelo de nuevo.";
                return response;
            }

            // Si las credenciales son correctas, mapeamos la respuesta de autenticación
            response.Id = user.Id;
            response.Email = user.Email!;
            response.UserName = user.UserName!;
            response.IsActive = user.IsActive;

            // Obtener los roles del usuario desde las tablas relacionales de Identity
            var roles = await _userManager.GetRolesAsync(user);
            response.Roles = roles.ToList();

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
            // Codificar el token para transportarlo de manera segura por la barra de direcciones URL
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Construye la ruta: https://midominio.com/Account/ConfirmEmail?userId=...&token=...
            var route = "/Account/ConfirmEmail";
            var url = $"{origin}{route}?userId={user.Id}&token={encodedToken}";
            return url;
        }
    }
}