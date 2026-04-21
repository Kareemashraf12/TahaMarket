using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class PaymentResponse
    {
        public Guid OrderId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string Message { get; set; }
    }
}
