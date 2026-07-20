using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Contexts;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;
using RealEstateApp.Infrastructure.Identity.Services;
using System.Security.Claims;
using System.Text;

namespace RealEstateApp.Infrastructure.Identity
{
    public static class ServiceRegistration
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("IdentityConnection"),
                b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            // Mantenemos la configuración de Cookies para la WebApp
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            #region JWT Configuration
            // Leemos la sección del JSON mapeándola a las validaciones de tokens
            services.Configure<Domain.Settings.JWTSettings>(configuration.GetSection("JWTSettings"));

            services.AddAuthentication(options =>
            {
                // El API usará de manera predeterminada JWT para autenticar y autorizar
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWTSettings:Issuer"],
                    ValidAudience = configuration["JWTSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]!)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
            #endregion

            services.AddScoped<IIdentitySeeds, IdentitySeeds>();
            services.AddScoped<IAccountService, AccountService>();
        }
    }
}