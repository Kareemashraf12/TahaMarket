using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class Delivery
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
        public bool IsOnline { get; set; } = false;

        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }

        public string VehicleType { get; set; }

        // ⬅ Add these for Auth
        public decimal Balance { get; set; } = 0;
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
