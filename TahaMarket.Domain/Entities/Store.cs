public class Store
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }

    public string Address { get; set; }
    public string ImageUrl { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public decimal MinimumOrderAmount { get; set; }
    public string Type { get; set; }

    
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }

    public ICollection<Category> Categories { get; set; }
}