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
            
            context.Database.EnsureCreated();

            // Check if Admin exists
            if (!context.Users.Any(u => u.UserType == "Admin"))
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Email = "krymk9920@gmail.com",
                    PhoneNumber = "01141286090",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    UserType = "Admin",
                    RefreshToken = null,
                    RefreshTokenExpiry = DateTime.MinValue
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}