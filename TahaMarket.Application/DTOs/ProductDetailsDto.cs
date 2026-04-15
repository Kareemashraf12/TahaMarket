using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ProductDetailsDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        // Store
        public Guid StoreId { get; set; }
        public string StoreName { get; set; }

        // Category
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Variants
        public List<ProductVariantDto> Variants { get; set; }

        // Price summary
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        // Rating Summary
        public RatingSummaryDto Rating { get; set; }
    }
}
