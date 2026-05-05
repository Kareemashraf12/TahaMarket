using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class CartItemAddOnPreviewDto
    {
        public Guid AddOnOptionId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
