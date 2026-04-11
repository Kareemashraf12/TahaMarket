using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class DeliveryTransaction
    {
        public Guid Id { get; set; }

        public Guid DeliveryId { get; set; }
        public Guid OrderId { get; set; }

        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Type { get; set; } // Delivery / Bonus / Penalty
    }
}
