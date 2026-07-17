using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RealEstateApp.Infrastructure.Identity.Contexts;

namespace RealEstateApp.Infrastructure.Identity
{
    public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();

            var connectionString = "Server=localhost;Database=RealEstateAppDb;Trusted_Connection=True;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));

            return new IdentityContext(optionsBuilder.Options);
        }
    }
}