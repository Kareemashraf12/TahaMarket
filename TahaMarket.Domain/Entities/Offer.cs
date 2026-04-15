using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Domain.Entities
{
    public class Offer
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }

        public decimal DiscountPercentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Target (Product / Category)
        public Guid TargetId { get; set; }
        public OfferTargetType TargetType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
