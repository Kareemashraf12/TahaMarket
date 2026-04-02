using System;
using System.Collections.Generic;

namespace TahaMarket.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Delivered, Cancelled
        public virtual List<OrderItem> Items { get; set; }
        public string Note { get; set; } // Optional note for delivery
    }
}