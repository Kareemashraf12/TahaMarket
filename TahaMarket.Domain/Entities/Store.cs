using TahaMarket.Domain.Entities;

public class Store
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public string Type { get; set; } = "Store"; // For Auth purpose
    public string Address { get; set; }
    public string ImageUrl { get; set; }

    // =========================
    // DESCRIPTION
    // =========================
    public string? Description { get; set; }

    // =========================
    // LOCATION
    // =========================
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // =========================
    // WORKING HOURS
    // =========================
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }

    // =========================
    // SECTION RELATION
    // =========================
    public Guid? StoreSectionId { get; set; }
    public StoreSection? StoreSection { get; set; }

    // =========================
    // BUSINESS RULES
    // =========================
    public decimal MinimumOrderAmount { get; set; }

    // =========================
    // AUTH
    // =========================
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }

    // =========================
    // NAVIGATION
    // =========================
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public List<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Product> Products { get; set; } = new List<Product>();


}