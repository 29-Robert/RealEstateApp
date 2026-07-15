using System.Threading.Tasks;

namespace RealEstateApp.Infrastructure.Identity.Seeds
{
    public interface IIdentitySeeds
    {
        Task SeedAsync();
    }
}