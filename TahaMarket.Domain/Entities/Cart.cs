using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid StoreId { get; set; }

        public List<CartItem> Items { get; set; } = new();
        public Store Store { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
