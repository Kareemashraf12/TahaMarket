using System.ComponentModel.DataAnnotations;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public string ImageUrl { get; set; }

    // 🔥 Relation with Category
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
}