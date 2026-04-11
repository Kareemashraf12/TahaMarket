using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class ResetPasswordRequest
    {
        public string PhoneNumber { get; set; }
        
        public string NewPassword { get; set; }
    }
}
