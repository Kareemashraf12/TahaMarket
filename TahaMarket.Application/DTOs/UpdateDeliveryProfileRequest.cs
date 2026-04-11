using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class UpdateDeliveryProfileRequest
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? VehicleType { get; set; }
        public IFormFile? Image { get; set; }
    }
}
