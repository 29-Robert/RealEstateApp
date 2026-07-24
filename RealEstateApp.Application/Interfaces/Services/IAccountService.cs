using RealEstateApp.Application.DTOs.Account;
using RealEstateApp.Application.ViewModels.Users;
using RealEstateApp.Core.Application.ViewModels.Account;

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
        Task<UserResponseDto?> GetUserByIdAsync(string id);
        
        Task ToggleAgentStatusAsync(string agentId);
        Task<List<AgentResponseDto>> GetAgentsListAsync();
        Task<AdminDashboardResponseDto> GetAdminDashboardStatsAsync();
        Task<RegisterResponseDto> DeleteAgentAsync(string agentId);
        Task<RegisterResponseDto> UpdateAdminOrDeveloperAsync(EditUserViewModel vm);
        Task<RegisterResponseDto> DeleteAdminOrDeveloperAsync(string id);
        Task ToggleUserStatusAsync(string id);

        Task<SaveProfileViewModel> GetUserProfileByIdAsync(string userId);
        Task<UpdateProfileResponse> UpdateProfileAsync(SaveProfileViewModel vm);
    }
}