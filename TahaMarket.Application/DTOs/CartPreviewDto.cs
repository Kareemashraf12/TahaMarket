using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class CartPreviewDto
    {
        public Guid CartId { get; set; }

        public string StoreName { get; set; }

        public List<CartItemPreviewDto> Items { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal DeliveryFee { get; set; }

        public decimal FinalPrice { get; set; }
    }
}
