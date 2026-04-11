using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class UpdateAddressRequest
    {
        public Guid AddressId { get; set; }

        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Street { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool? IsDefault { get; set; }
    }
}
