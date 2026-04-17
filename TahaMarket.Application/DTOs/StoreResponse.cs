using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    
        public class StoreResponse
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public string ImageUrl { get; set; }
            public string? Description { get; set; }

            
            public double AverageRating { get; set; }
        }

}
