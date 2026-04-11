using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class DeliveryRating
    {
        public Guid Id { get; set; }

        public Guid DeliveryId { get; set; }
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }

        public int Rate { get; set; }
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
