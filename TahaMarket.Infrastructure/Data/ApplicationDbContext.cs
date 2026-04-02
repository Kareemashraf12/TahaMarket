using System;
using Microsoft.EntityFrameworkCore;
using TahaMarket.Domain.Entities;

namespace TahaMarket.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ------------------- DbSets -------------------

        public DbSet<User> Users { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }

        public DbSet<StoreRating> StoreRatings { get; set; }
        public DbSet<ProductRating> ProductRatings { get; set; }

        // ------------------- Model Creating -------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ------------------- Store -> Categories -------------------
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Store)
                .WithMany(s => s.Categories)
                .HasForeignKey(c => c.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------- Category -> Products -------------------
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------- Store -> Orders -------------------
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany()
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------- User -> Orders -------------------
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------- Order -> OrderItems -------------------
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ------------------- Product -> OrderItems -------------------
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------- User -> Addresses -------------------
          
            modelBuilder.Entity<UserAddress>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        

        // ------------------- Unique Constraints -------------------
        modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Store>()
                .HasIndex(s => s.PhoneNumber)
                .IsUnique();

            // ------------------- Admin Seed -------------------
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Admin",
                Email = "krymk9920@gmail.com",
                PhoneNumber = "01141286090",
                PasswordHash = "$2a$11$9e.ldSEf8MHdxc5C1fP3jumXJ0Z4PUqeuS7jHa8DRIVxFhAN5bCJK",
                UserType = "Admin",
                RefreshToken = null,
                RefreshTokenExpiry = DateTime.MinValue
            });
        }
    }
}