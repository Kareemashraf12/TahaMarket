using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class DeliveryOrder
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Guid DeliveryId { get; set; }

        public DateTime AssignedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PickedAt { get; set; }         // rename from PickedUpAt
        public DateTime? DeliveredAt { get; set; }

        public string Status { get; set; }

        public decimal DeliveryFee { get; set; }
        public double DistanceKm { get; set; }

        public string Notes { get; set; }

        // ⬇ Add navigation properties
        public virtual Order Order { get; set; }
        public virtual Delivery Delivery { get; set; }
    }
}
