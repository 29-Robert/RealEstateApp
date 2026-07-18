using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.ViewModels.Users;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<AuthenticationResponseDto> AuthenticateAsync(LoginViewModel loginVm);
        Task<RegisterResponseDto> RegisterUserAsync(RegisterViewModel registerVm, string origin);
        Task<string> ConfirmEmailAsync(string userId, string token);
        Task SignOutAsync();
        Task<RegisterResponseDto> RegisterAdminOrDeveloperAsync(RegisterViewModel request, string role);
        Task<List<UserResponseDto>> GetUsersInRoleAsync(string role);
        
        Task ToggleAgentStatusAsync(string agentId);
        Task<List<AgentResponseDto>> GetAgentsListAsync();


    }
}