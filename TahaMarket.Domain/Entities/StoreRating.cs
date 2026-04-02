using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class StoreRating
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Range(1, 5, ErrorMessage = "Value must be between 1 and 5.")]
        public int Value { get; set; } // 1 → 5

        public string? Comment { get; set; }

        public Guid StoreId { get; set; }
        public Store Store { get; set; }

        public Guid UserId { get; set; }
    }
}
