using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class OfferDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public decimal DiscountPercentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public OfferTargetType TargetType { get; set; }
        public Guid TargetId { get; set; }
    }
}
