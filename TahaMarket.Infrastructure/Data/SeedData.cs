using System;
using System.Linq;
using TahaMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TahaMarket.Infrastructure.Data
{
    public static class SeedData
    {
        public static void EnsureAdminExists(ApplicationDbContext context)
        {
            // Ensure database is created (prefer migrations in production)
            context.Database.EnsureCreated();

            // Check if Admin already exists (case-insensitive safety)
            var adminExists = context.Users
                .Any(u => u.UserType.ToLower() == "admin");

            if (!adminExists)
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    PhoneNumber = "01141286090",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),

                    IsVerified = true,
                    UserType = "Admin",

                    ImageUrl = "/images/users/user.png",

                    RefreshToken = null,
                    RefreshTokenExpiry = null,

                    CanResetPassword = false,
                    ResetAllowedUntil = null
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}