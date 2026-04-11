using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class OtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Code { get; set; }
    }
}
