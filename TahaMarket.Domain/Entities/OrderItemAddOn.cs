using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class OrderItemAddOn
    {
        public Guid Id { get; set; }

        public Guid OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
