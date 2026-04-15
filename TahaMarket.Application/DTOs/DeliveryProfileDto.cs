using System;

namespace TahaMarket.Application.DTOs
{
    public class DeliveryProfileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string VehicleType { get; set; }

        public bool IsAvailable { get; set; }
        public bool IsOnline { get; set; }

        public decimal Balance { get; set; }

        public string ImageUrl { get; set; }
    }
}