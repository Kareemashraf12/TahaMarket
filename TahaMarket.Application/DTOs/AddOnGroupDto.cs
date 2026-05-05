using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class AddOnGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<AddOnOptionDto> Options { get; set; } = new();
    }
}
