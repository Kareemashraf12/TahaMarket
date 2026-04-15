using System.ComponentModel.DataAnnotations;
using TahaMarket.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }
    public string? Description { get; set; }

    public string ImageUrl { get; set; }

    // Relations
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }

    public Guid StoreId { get; set; }
    public Store Store { get; set; }

    // Navigation
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}