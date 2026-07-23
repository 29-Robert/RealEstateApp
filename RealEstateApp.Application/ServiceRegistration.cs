using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Services;


namespace RealEstateApp.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
            services.AddTransient(typeof(IGenericService<,,>), typeof(GenericService<,,>));
            services.AddTransient<IPropertyTypeService, PropertyTypeService>();
            services.AddTransient<ISaleTypeService, SaleTypeService>();
            services.AddTransient<IImprovementService, ImprovementService>();
            services.AddTransient<IPropertyService, PropertyService>();
            services.AddTransient<IFavoriteService, FavoriteService>();
        }
    }
}