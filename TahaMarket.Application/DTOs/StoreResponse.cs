using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    namespace TahaMarket.Application.DTOs
    {
        public class StoreResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string PhoneNumber { get; set; }
            public decimal MinimumOrderAmount { get; set; }
            public string Type { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}
