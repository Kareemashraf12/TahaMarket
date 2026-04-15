using System;

namespace TahaMarket.Application.DTOs
{
    public class DeliveryListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string VehicleType { get; set; }

        public bool IsAvailable { get; set; }
        public bool IsOnline { get; set; }

        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
    }
}