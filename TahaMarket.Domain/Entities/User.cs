using TahaMarket.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PhoneNumber { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public string? Name { get; set; }
    public bool IsVerified { get; set; } = false;
    public string UserType { get; set; } = "Customer";

    public string? ImageUrl { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    //  Reset Password Flow
    public bool CanResetPassword { get; set; } = false;
    public DateTime? ResetAllowedUntil { get; set; }

    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
}