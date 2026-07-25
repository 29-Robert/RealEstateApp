using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyType> PropertyTypes { get; set; }
        public DbSet<SaleType> SaleTypes { get; set; }
        public DbSet<Improvement> Improvements { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Offer> Offers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Offer>()
                .ToTable("Offer")
                .Property(o => o.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Property)
                .WithMany(p => p.Offers)
                .HasForeignKey(o => o.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .ToTable("Properties")
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PropertyType>()
                .ToTable("PropertyTypes");

            modelBuilder.Entity<SaleType>()
                .ToTable("SaleTypes");

            modelBuilder.Entity<Improvement>()
                .ToTable("Improvements");

            modelBuilder.Entity<PropertyImage>()
                .ToTable("PropertyImages");

            modelBuilder.Entity<PropertyType>()
                .HasMany<Property>(pt => pt.Properties)
                .WithOne(p => p.PropertyType)
                .HasForeignKey(p => p.PropertyTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleType>()
                .HasMany<Property>(st => st.Properties)
                .WithOne(p => p.SaleType)
                .HasForeignKey(p => p.SaleTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasMany<Improvement>(p => p.Improvements)
                .WithMany(i => i.Properties)
                .UsingEntity(j => j.ToTable("PropertyImprovements"));

            modelBuilder.Entity<Property>()
                .HasMany<PropertyImage>(p => p.PropertyImages)
                .WithOne(pi => pi.Property)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .ToTable("Messages")
                .HasOne(m => m.Property)
                .WithMany()
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
