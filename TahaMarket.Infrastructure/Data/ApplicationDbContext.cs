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
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<StoreSection> storeSections { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryOrder> DeliveryOrders { get; set; }
        public DbSet<DeliveryTransaction> DeliveryTransactions { get; set; }
        public DbSet<DeliveryPricing> DeliveryPricings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // ------------------- Model Creating -------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // STORE -> CATEGORY (KEEP CASCADE OK)
            // =========================================================
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Store)
                .WithMany(s => s.Categories)
                .HasForeignKey(c => c.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================================================
            // CATEGORY -> PRODUCTS (REMOVE CASCADE ISSUE)
            // =========================================================
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================================================
            // STORE -> PRODUCTS (NO ACTION TO AVOID CYCLE)
            // =========================================================
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Store)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================================================
            // STORE -> ORDERS
            // =========================================================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany()
                .HasForeignKey(o => o.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // USER -> ORDERS
            // =========================================================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // ORDER -> ORDER ITEMS
            // =========================================================
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // USER -> ADDRESSES
            // =========================================================
            modelBuilder.Entity<UserAddress>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.UserAddress)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Delivery)
                .WithMany()
                .HasForeignKey(o => o.DeliveryId)
                .OnDelete(DeleteBehavior.SetNull);

            // =========================================================
            // FAVORITES
            // =========================================================
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================================================
            // PRODUCT VARIANTS
            // =========================================================
            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId);

            // =========================================================
            // STORE SECTION
            // =========================================================
            modelBuilder.Entity<Store>()
                .HasOne(s => s.StoreSection)
                .WithMany(ss => ss.Stores)
                .HasForeignKey(s => s.StoreSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // UNIQUE INDEXES
            // =========================================================
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Store>()
                .HasIndex(s => s.PhoneNumber)
                .IsUnique();


            // =========================================================
            // OTHER INDEXES
            // =========================================================
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => f.UserId);

            modelBuilder.Entity<OtpCode>()
                .HasIndex(o => o.PhoneNumber);

            modelBuilder.Entity<DeliveryOrder>()
                .HasIndex(x => x.OrderId);

            modelBuilder.Entity<DeliveryOrder>()
                .HasIndex(x => x.DeliveryId);

            modelBuilder.Entity<DeliveryTransaction>()
                .HasIndex(x => x.DeliveryId);

            // =========================================================
            // ENUM CONVERSION
            // =========================================================
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.UserType)
                .HasConversion<int>();

            // ------------------- Admin Seed -------------------
            //modelBuilder.Entity<User>().HasData(new User
            //{
            //        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            //        Name = "Admin",
            //        ImageUrl = "/images/users/user.png",
            //        PhoneNumber = "01141286090",
            //        PasswordHash = "$2a$11$9e.ldSEf8MHdxc5C1fP3jumXJ0Z4PUqeuS7jHa8DRIVxFhAN5bCJK",
            //        IsPhoneVerified = true,
            //        UserType = "Admin",
            //        RefreshToken = null,
            //        RefreshTokenExpiry = DateTime.MinValue
            //});
        }
    }
}