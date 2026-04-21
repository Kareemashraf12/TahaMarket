using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class AssignOrderWithPaymentRequest
    {
        public Guid OrderId { get; set; }
        public Guid DeliveryId { get; set; }
        public AssignedByType AssignedBy { get; set; }
        public decimal? ManualFee { get; set; }

        public PaymentMethod PaymentMethod { get; set; } // NEW
    }
}
