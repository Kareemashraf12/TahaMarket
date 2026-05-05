using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class CartItemResponse
    {
        public Guid CartItemId { get; set; }

        public Guid ProductId { get; set; }
        public string ProductName { get; set; }

        public Guid VariantId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total => Price * Quantity;
    }
}
