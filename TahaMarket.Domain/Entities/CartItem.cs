using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }

        public Guid CartId { get; set; }
        public Cart Cart { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public ProductVariant Variant { get; set; }

        public string? Note { get; set; }
        public Guid VariantId { get; set; }

        public List<CartItemAddOn> AddOns { get; set; } = new();
    }
}
