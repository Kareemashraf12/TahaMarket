using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class PaymentUrlResponse
    {
        public Guid OrderId { get; set; }
        public string PaymentUrl { get; set; }
    }
}
