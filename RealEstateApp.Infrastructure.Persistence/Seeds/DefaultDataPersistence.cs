using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Seeds
{
    public static class DefaultData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // 1. Sembrar Tipos de Propiedad si la tabla está vacía
            if (!await context.Set<PropertyType>().AnyAsync())
            {
                await context.Set<PropertyType>().AddRangeAsync(
                    new PropertyType { Name = "Apartamento", Description = "Unidad residencial en edificio o condominio" },
                    new PropertyType { Name = "Casa", Description = "Vivienda unifamiliar independiente" },
                    new PropertyType { Name = "Villa", Description = "Residencia de lujo con terreno amplio" },
                    new PropertyType { Name = "Terreno", Description = "Lote de tierra para construcción" }
                );
            }

            // 2. Sembrar Tipos de Venta si la tabla está vacía
            if (!await context.Set<SaleType>().AnyAsync())
            {
                await context.Set<SaleType>().AddRangeAsync(
                    new SaleType { Name = "Venta", Description = "Propiedad disponible para compra definitiva" },
                    new SaleType { Name = "Alquiler", Description = "Propiedad disponible para renta mensual" }
                );
            }

            // 3. Sembrar Mejoras (Improvements) si la tabla está vacía
            if (!await context.Set<Improvement>().AnyAsync())
            {
                await context.Set<Improvement>().AddRangeAsync(
                    new Improvement { Name = "Piscina", Description = "Piscina privada o común" },
                    new Improvement { Name = "Gimnasio", Description = "Área de ejercicios equipada" },
                    new Improvement { Name = "Seguridad 24/7", Description = "Vigilancia constante y acceso controlado" },
                    new Improvement { Name = "Planta Eléctrica", Description = "Generador eléctrico de respaldo" }
                );
            }

            // Guardar todos los cambios en la base de datos
            await context.SaveChangesAsync();
        }
    }
}
