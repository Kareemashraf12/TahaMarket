using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public decimal OldPrice { get; set; }
        public decimal FinalPrice { get; set; }

        public bool HasDiscount { get; set; }
        public decimal DiscountPercentage { get; set; }

        public string ImageUrl { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }

        
        public List<ProductVariantResponse> Variants { get; set; }
    }
}
