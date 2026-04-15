using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class SearchRequest
    {
        public string Query { get; set; }
        public SearchType Type { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
