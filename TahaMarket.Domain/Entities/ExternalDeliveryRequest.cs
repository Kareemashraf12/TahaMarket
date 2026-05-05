using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class ExternalDeliveryRequest
    {
        public Guid Id { get; set; }

        public Guid StoreId { get; set; }
        public Store Store { get; set; }

        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsAssigned { get; set; } = false;
    }
}
