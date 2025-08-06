using Microsoft.EntityFrameworkCore;
using TechStore_BE.Models;

namespace TechStore_BE.DataConnection
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Brands> Brands { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Carts> Carts { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Order_Details> Order_Details { get; set; }
        public DbSet<ProductReviews> ProductReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Products>()
                .HasOne(p => p.Brand)
                .WithMany()
                .HasForeignKey(p => p.brand_id)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Products>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.category_id)
                .OnDelete(DeleteBehavior.NoAction);

        }

    }
}