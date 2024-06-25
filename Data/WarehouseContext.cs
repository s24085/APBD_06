using Microsoft.EntityFrameworkCore;
using APBD_06.Models;

namespace APBD_06.Data
{
    public class WarehouseContext : DbContext
    {
        public WarehouseContext(DbContextOptions<WarehouseContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductWarehouse> ProductWarehouses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(o => o.IdOrder);
            modelBuilder.Entity<Product>().HasKey(p => p.IdProduct);
            modelBuilder.Entity<Warehouse>().HasKey(w => w.IdWarehouse);
            modelBuilder.Entity<ProductWarehouse>().HasKey(pw => pw.IdProductWarehouse);

            modelBuilder.Entity<Order>().ToTable("Order");
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<Warehouse>().ToTable("Warehouse");
            modelBuilder.Entity<ProductWarehouse>().ToTable("Product_Warehouse");
        }
    }
}