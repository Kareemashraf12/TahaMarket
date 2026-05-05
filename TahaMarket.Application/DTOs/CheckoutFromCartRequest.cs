using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class CheckoutFromCartRequest
    {
        public Guid CartId { get; set; }
        public Guid AddressId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
