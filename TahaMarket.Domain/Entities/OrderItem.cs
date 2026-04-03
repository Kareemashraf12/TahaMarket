namespace TahaMarket.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Price at the time of order
        public string? Note { get; set; }

        public Order Order  { get; set; }

        public Guid OrderId { get; set; }
    }
}