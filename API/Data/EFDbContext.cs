using API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class EFDbContext : DbContext
    {
        public EFDbContext(DbContextOptions<EFDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API (nếu cần)
            //modelBuilder.Entity<Category>().ToTable("tblCategory");
            //modelBuilder.Entity<Product>().ToTable("tblProduct");

            // Composite key
            //modelBuilder.Entity<UserRole>()
            //    .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Relationships            
            /*
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId);
            */
        }

    }
}
