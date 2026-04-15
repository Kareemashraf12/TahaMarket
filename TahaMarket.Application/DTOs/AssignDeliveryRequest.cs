using System;

namespace TahaMarket.Application.DTOs
{
    public class AssignDeliveryRequest
    {
        public Guid OrderId { get; set; }
        public Guid DeliveryId { get; set; }

        // optional: store can set fee manually or let system calculate
        public decimal? DeliveryFee { get; set; }
    }
}