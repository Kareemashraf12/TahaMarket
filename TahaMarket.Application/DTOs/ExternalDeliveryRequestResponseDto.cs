using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ExternalDeliveryRequestResponseDto
    {
        public Guid Id { get; set; }

        public string StoreName { get; set; }

        public string Address { get; set; }

        public bool IsAssigned { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
