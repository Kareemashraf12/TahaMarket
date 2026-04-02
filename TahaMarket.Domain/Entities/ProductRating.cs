using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class ProductRating
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Value { get; set; } // 1 → 5

        public string? Comment { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid UserId { get; set; }
    }
}
