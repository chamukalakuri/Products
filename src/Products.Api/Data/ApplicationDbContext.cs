using Microsoft.EntityFrameworkCore;
using Products.Api.Models;

namespace Products.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.HasIndex(e => e.Color);  // Index on Color for better query performance
            });
        }
    }
}
