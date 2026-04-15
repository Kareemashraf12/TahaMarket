using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ProductReviewsResponse
    {
        public double AverageRating { get; set; }

        public int TotalCount { get; set; }

        public Dictionary<int, int> RatingBreakdown { get; set; }

        public List<ProductReviewDto> Reviews { get; set; }
    }
}
