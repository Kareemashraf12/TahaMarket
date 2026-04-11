using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class OtpCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string PhoneNumber { get; set; }
        public string Code { get; set; }

        public DateTime Expiry { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
