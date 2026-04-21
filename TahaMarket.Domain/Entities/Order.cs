using System;
using System.Collections.Generic;
using TahaMarket.Domain.Enums;

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
            public virtual UserAddress UserAddress { get; set; }

            public Guid? DeliveryId { get; set; }
            public virtual Delivery Delivery { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public OrderStatus Status { get; set; } = OrderStatus.Pending;

            public virtual List<OrderItem> Items { get; set; } = new List<OrderItem>();
            public string? Note { get; set; }

            public decimal TotalPrice { get; set; }
            public decimal DeliveryFee { get; set; }
            public decimal FinalPrice { get; set; }

            // =========================
            // TIMESTAMPS
            // =========================
            public DateTime? AcceptedAt { get; set; }     // Preparing
            public DateTime? ReadyAt { get; set; }        // Ready
            public DateTime? AssignedAt { get; set; }     // Assigned
            public DateTime? PickedAt { get; set; }       // Picked (OutForDelivery)
            public DateTime? DeliveredAt { get; set; }    // Delivered

            // =========================
            // DELIVERY RELATION
            // =========================
            public virtual List<DeliveryOrder> DeliveryOrders { get; set; } = new();

            // =========================
            // PAYMENT
            // =========================
            public PaymentMethod PaymentMethod { get; set; } // COD / Online
            public PaymentStatus PaymentStatus { get; set; } // Pending / Paid / Failed
            public string? PaymentTransactionId { get; set; }
        }
    
}