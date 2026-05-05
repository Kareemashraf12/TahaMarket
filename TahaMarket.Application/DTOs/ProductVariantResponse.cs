using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ProductVariantResponse
    {
        public Guid Id { get; set; }
        public string Size { get; set; } // Small / Large
        public decimal Price { get; set; }
        public bool IsStockTracked { get; set; } = true;
        public int? StockQuantity { get; set; }
        public List<ProductVariantResponse> Variants { get; set; }
    }
}
