using Microsoft.AspNetCore.Identity;
using RealEstateApp.Application;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Identity;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;
using RealEstateApp.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddSharedLayerIoc(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration); // Registra base de datos de Identity, Roles y AccountService

// Configuración de Controladores con Vistas (MVC)
builder.Services.AddControllersWithViews();

// Configuración de Sesión en Memoria (Opcional, pero útil si guardas datos del usuario temporalmente)
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
        var seeder = services.GetRequiredService<IIdentitySeeds>();
        // Ejecución asíncrona del Sembrado de Roles y Usuarios (Admin, Agente, Cliente) por defecto
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al ejecutar el sembrado (Seed) de datos de Identity.");
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

// Activa el almacenamiento de sesiones en el servidor
app.UseSession();

// ¡OJO! Seguridad: Authentication SIEMPRE debe ejecutarse estrictamente antes de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}") // Ajustamos la ruta inicial para que por defecto los mande a loguearse
    .WithStaticAssets();

app.Run();