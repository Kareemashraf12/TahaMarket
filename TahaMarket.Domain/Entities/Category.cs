using System.ComponentModel.DataAnnotations;


public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; }

    // 🔥 Relation with Store
    public Guid StoreId { get; set; }
    public Store Store { get; set; }

    // 🔥 Relation with Products
    public ICollection<Product> Products { get; set; } = new List<Product>();
}