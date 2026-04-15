using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PhoneNumber { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public string? Name { get; set; }
    public bool IsVerified { get; set; } = false;

    public UserType UserType { get; set; }   // FIXED

    public string? ImageUrl { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public bool CanResetPassword { get; set; } = false;
    public DateTime? ResetAllowedUntil { get; set; }

    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
}