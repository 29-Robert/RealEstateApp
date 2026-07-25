using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Application;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Identity;
using RealEstateApp.Infrastructure.Identity.Contexts;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;
using RealEstateApp.Infrastructure.Shared;
using RealEstateApp.Infrastructure.Persistence.Seeds;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApplicationLayer();
    builder.Services.AddPersistenceInfrastructure(builder.Configuration);
    builder.Services.AddSharedLayerIoc(builder.Configuration);
    builder.Services.AddIdentityInfrastructure(builder.Configuration); // Registra base de datos de Identity, Roles y AccountService

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

    builder.Services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
    {
        options.DefaultScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme;
    });

    // Configuración de Controladores con Vistas (MVC)
    builder.Services.AddControllersWithViews();

    // Configuración de Sesión en Memoria
    builder.Services.AddSession(opt =>
    {
        opt.IdleTimeout = TimeSpan.FromMinutes(60);
        opt.Cookie.HttpOnly = true;
        opt.Cookie.IsEssential = true;
    });

    var app = builder.Build();

    // ==========================================
    // SEMBRADO AUTOMÁTICO DE DATOS (SEEDS)
    // ==========================================
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            // 1. Aplicar migraciones automáticas para ApplicationDbContext
            var applicationContext = services.GetRequiredService<ApplicationDbContext>();
            await applicationContext.Database.MigrateAsync();

            // 2. Sembrado de datos del Negocio (Tipos de Propiedad, Venta y Mejoras) 👈 AQUÍ
            await DefaultData.SeedAsync(applicationContext);

            // 3. Aplicar migraciones automáticamente para IdentityContext
            var identityContext = services.GetRequiredService<IdentityContext>();
            await identityContext.Database.MigrateAsync();

            // 4. Sembrado de Roles y Usuarios por defecto
            var seeder = services.GetRequiredService<IIdentitySeeds>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocurrió un error al ejecutar las migraciones o el sembrado (Seed) de datos.");
        }
    }

    // ==========================================
    // MIDDLEWARES DE LA APLICACIÓN (PIPELINE HTTP)
    // ==========================================
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseSession();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name != "HostAbortedException")
{
    Console.WriteLine("==================================================");
    Console.WriteLine("FATAL CRASH DETECTED IN STARTUP:");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("==================================================");
    System.Threading.Thread.Sleep(5000);
    throw;
}