using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ProductReviewDto
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }
        public string? UserImage { get; set; }

        public int Value { get; set; } // 1 - 5

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
