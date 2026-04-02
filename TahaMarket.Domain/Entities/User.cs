using TahaMarket.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PhoneNumber { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public string? Name { get; set; }
    public string? Email { get; set; }

    
    public string UserType { get; set; } = "Customer";

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    
    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();


    public ICollection<CreateRatingRequest> Ratings { get; set; } = new List<CreateRatingRequest>();
}