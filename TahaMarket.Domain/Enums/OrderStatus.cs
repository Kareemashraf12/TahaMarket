using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public enum OrderStatus
    {
        Pending = 0,        // created
        Paid = 1,           // user paid (COD or online)
        Preparing = 2,      // store preparing
        Ready = 3,          // ready for pickup
        Assigned = 4,       // assigned to delivery
        Picked = 5,         // delivery picked it
        Delivered = 6,      // delivered
        Cancelled = 7       // cancelled
    }
}
