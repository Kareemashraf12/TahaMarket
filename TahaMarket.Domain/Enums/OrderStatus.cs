using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public enum OrderStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Assigned = 3,
        Picked = 4,
        Delivered = 5,
        Cancelled = 6
    }
}
