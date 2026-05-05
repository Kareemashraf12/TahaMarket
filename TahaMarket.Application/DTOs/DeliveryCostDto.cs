using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class DeliveryCostDto
    {
        public double DistanceKm { get; set; }
        public decimal BaseDeliveryFee { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal TotalDeliveryCost { get; set; }
    }
}
