using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class AddToCartRequest
    {
        public Guid StoreId { get; set; }
        public List<AddToCartItemDto> Items { get; set; }
    }
}
