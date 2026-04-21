using System;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Domain.Entities
{
    public class DeliveryOrder
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; }

        public Guid DeliveryId { get; set; }
        public virtual Delivery Delivery { get; set; }

      
        public decimal DeliveryFee { get; set; }

   
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

       
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PickedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}