using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class UpdateCartItemRequest
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
