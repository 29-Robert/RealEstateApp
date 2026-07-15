<<<<<<< HEAD
using RealEstateApp.Application;
using RealEstateApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);

=======
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Infrastructure.Identity; 
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;
using RealEstateApp.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

>>>>>>> c3a55be2a9f19f5c5204936f535dd32ed72c0088
builder.Services.AddControllersWithViews();

builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true; 
});

builder.Services.AddSharedLayerIoc(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration); 

var app = builder.Build();

<<<<<<< HEAD
=======
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<IIdentitySeeds>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al ejecutar el sembrado (Seed) de datos de Identity.");
    }
}

>>>>>>> c3a55be2a9f19f5c5204936f535dd32ed72c0088
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

<<<<<<< HEAD
app.UseAuthentication(); 
=======
// ¡IMPORTANTE! El middleware de sesión debe ir antes de la autenticación
app.UseSession();

// ¡IMPORTANTE! Para que Identity reconozca los usuarios logueados, debes usar Authentication ANTES de Authorization
app.UseAuthentication();
>>>>>>> c3a55be2a9f19f5c5204936f535dd32ed72c0088
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();