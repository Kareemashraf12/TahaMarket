using System;

namespace TahaMarket.Application.DTOs
{
    public class DeliveryOrderResponse
    {
        public Guid OrderId { get; set; }
        public Guid DeliveryId { get; set; }

        public string DeliveryName { get; set; }
        public string StoreName { get; set; }

        public decimal DeliveryFee { get; set; }

        public string Status { get; set; }

        public DateTime AssignedAt { get; set; }
    }
}