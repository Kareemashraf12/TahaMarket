using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class CartItemAddOn
    {
        public Guid Id { get; set; }

        public Guid CartItemId { get; set; }
        public CartItem CartItem { get; set; }

        public Guid AddOnOptionId { get; set; }

        
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
