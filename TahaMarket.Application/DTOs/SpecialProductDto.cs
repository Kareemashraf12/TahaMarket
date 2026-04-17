using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class SpecialProductDto
    {
        public string Label { get; set; } 

        public Guid ProductId { get; set; }
        public string ProductName { get; set; }

        public string ImageUrl { get; set; }
        public Guid StoreId { get; set; }
        public Guid CategoryId { get; set; }

        public double AverageRating { get; set; }
    }
}
