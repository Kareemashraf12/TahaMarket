using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class CreateOrderRequest
    {
        public Guid StoreId { get; set; }
        public Guid AddressId { get; set; }
        public List<OrderItemRequest> Items { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Note { get; set; }
    }
}
