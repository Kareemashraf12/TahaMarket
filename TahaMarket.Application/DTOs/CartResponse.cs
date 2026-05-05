using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class CartResponse
    {
        public Guid CartId { get; set; }
        public Guid StoreId { get; set; }

        public List<CartItemResponse> Items { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
    }
}
