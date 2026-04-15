using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public static class SeedData
{
    public static void EnsureAdminExists(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        var adminExists = context.Users
            .Any(u => u.UserType == UserType.Admin);

        if (!adminExists)
        {
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                PhoneNumber = "01141286090",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),

                IsVerified = true,
                UserType = UserType.Admin,   //  FIXED

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