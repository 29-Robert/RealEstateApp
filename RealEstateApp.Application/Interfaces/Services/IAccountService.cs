using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.ViewModels.Users;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<AuthenticationResponse> AuthenticateAsync(LoginViewModel loginVm);
        Task<RegisterResponse> RegisterUserAsync(RegisterViewModel registerVm, string origin);
        Task<string> ConfirmEmailAsync(string userId, string token);
        Task SignOutAsync();
        Task<RegisterResponse> RegisterAdminOrDeveloperAsync(RegisterViewModel request, string role);

    }
}