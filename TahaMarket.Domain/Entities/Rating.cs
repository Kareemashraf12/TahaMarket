using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Domain.Entities
{
    public class Rating
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Value { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //  Generic Target
        public Guid TargetId { get; set; }
        public RatingTargetType TargetType { get; set; }

        // User
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
