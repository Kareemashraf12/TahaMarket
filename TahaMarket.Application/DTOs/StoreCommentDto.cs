using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class StoreCommentDto
    {
        public string UserName { get; set; }
        public string UserImage { get; set; }

        public int Value { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
