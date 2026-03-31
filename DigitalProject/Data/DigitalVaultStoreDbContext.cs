using DigitalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Data
{
    public class DigitalVaultStoreDbContext : DbContext
    {
        public DigitalVaultStoreDbContext(DbContextOptions<DigitalVaultStoreDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Review>()
                .HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(e => e.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(e => e.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);  // 模擬用，交易實作後移除

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.PaymentCode)
                      .HasMaxLength(20);

                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(10,2)");

                //entity.Property(e => e.Provider)
                //      .HasConversion<string>();

                //entity.Property(e => e.Status)
                //      .HasConversion<string>();

                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Payments)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.VoidByUser)
                      .WithMany(u => u.VoidedPayments)
                      .HasForeignKey(e => e.VoidByUserId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.NoAction);
            });
    }
    }
}