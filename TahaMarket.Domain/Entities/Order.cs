using System;
using System.Collections.Generic;

namespace TahaMarket.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public Guid StoreId { get; set; }
        public virtual Store Store { get; set; }

        public Guid AddressId { get; set; }
        public virtual UserAddress UserAddress { get; set; }   // ⬅ rename from Address

        public Guid? DeliveryId { get; set; }
        public virtual Delivery Delivery { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public virtual List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string? Note { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal FinalPrice { get; set; }

        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public virtual List<DeliveryOrder> DeliveryOrders { get; set; } = new();
    }
}