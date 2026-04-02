using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class CreateRatingRequest
    {
        public int Value { get; set; } // 1 → 5
        public string? Comment { get; set; }
    }
}
