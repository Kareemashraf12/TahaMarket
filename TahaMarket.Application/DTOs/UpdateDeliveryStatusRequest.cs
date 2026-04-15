using System;

namespace TahaMarket.Application.DTOs
{
    public class UpdateDeliveryStatusRequest
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        // "Picked" or "Delivered"
    }
}