using Microsoft.EntityFrameworkCore;
using pedidoweb3.Models;

namespace pedidoweb3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Email unico
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //Estado como string legible
            modelBuilder.Entity<Order>()
                .Property(o => o.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            //Precisiones decimales
            modelBuilder.Entity<Product>()
                .Property(p => p.Precio).HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.Total).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Subtotal).HasPrecision(18, 2);

            //relaciones
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cliente)
                .WithMany(u => u.Orders!)
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.OrderItems!)
                .HasForeignKey(i => i.ProductId);
        }
    }
}
