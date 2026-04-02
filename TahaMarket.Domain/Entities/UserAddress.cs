public class UserAddress
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string City { get; set; } = null!;
    public string Area { get; set; } = null!;
    public string Street { get; set; } = null!;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public bool IsDefault { get; set; } = false;
}