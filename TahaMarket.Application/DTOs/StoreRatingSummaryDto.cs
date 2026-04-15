using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class StoreRatingSummaryDto
    {
        public double Average { get; set; }
        public int Count { get; set; }

        public int Star1 { get; set; }
        public int Star2 { get; set; }
        public int Star3 { get; set; }
        public int Star4 { get; set; }
        public int Star5 { get; set; }
    }
}
