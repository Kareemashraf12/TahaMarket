using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class AddToCartItemDto
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string? Note { get; set; }
        public int Quantity { get; set; }
        public List<Guid>? AddOnOptionIds { get; set; }
    }
}
