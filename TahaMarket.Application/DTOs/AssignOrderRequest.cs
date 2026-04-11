using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class AssignOrderRequest
    {
        public Guid OrderId { get; set; }
        public Guid DeliveryId { get; set; }
    }
}
