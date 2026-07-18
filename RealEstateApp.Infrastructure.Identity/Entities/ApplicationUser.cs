using Microsoft.AspNetCore.Identity;

namespace RealEstateApp.Infrastructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool IsActive { get; set; }
        public string? PhotoPath { get; set; }
        public string? CardId { get; set; }
    }
}