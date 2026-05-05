using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ProductListDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public decimal OldPrice { get; set; }

        public decimal FinalPrice { get; set; }

        public bool HasStock { get; set; }
        public int? StockQuantity { get; set; }
        public bool HasDiscount { get; set; }

        public decimal DiscountPercentage { get; set; }

        public double AverageRating { get; set; }

        public int RatingsCount { get; set; }
        public List<ProductVariantResponse> Variants { get; set; }
    }
}
