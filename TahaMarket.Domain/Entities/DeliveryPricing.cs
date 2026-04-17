using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class DeliveryPricing
    {
        public int Id { get; set; }

        public decimal PricePerKm { get; set; }   
        public decimal BaseFee { get; set; }      

        public decimal MinFee { get; set; }      
        public decimal MaxFee { get; set; }      

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
