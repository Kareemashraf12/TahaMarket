using System;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class AssignOrderRequest
    {
        public Guid OrderId { get; set; }
        public Guid DeliveryId { get; set; }

        // who is assigning (Admin / Store)
        public AssignedByType AssignedBy { get; set; }

        // optional override fee
        public decimal? ManualFee { get; set; }
    }
}