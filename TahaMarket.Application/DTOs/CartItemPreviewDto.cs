using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class CartItemPreviewDto
    {
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string ImageUrl { get; set; }

        public int Quantity { get; set; }

        public decimal DiscountedPrice { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }

        public string? Note { get; set; }

        public List<CartItemAddOnPreviewDto> AddOns { get; set; }
    }
}
