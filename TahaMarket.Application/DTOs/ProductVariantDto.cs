using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ProductVariantDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } // Small / Medium / Large

        public decimal Price { get; set; }
    }
}
