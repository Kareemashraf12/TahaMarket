using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class AddOnResponse
    {
        public List<AddOnGroupDto> AddOns { get; set; } = new();
    }
}
