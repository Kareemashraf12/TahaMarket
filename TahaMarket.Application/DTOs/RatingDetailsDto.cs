using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class RatingDetailsDto
    {
        public RatingSummaryDto Summary { get; set; }
        public PagedResult<RatingCommentDto> Comments { get; set; }
    }
}
