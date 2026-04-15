using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class ProductVariant
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } // Small / Medium / Large

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
