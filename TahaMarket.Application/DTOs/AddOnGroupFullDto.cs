using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class AddOnGroupFullDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid? ProductId { get; set; }
        public Guid? StoreId { get; set; }

        public List<AddOnOptionDto> Options { get; set; } = new();
    }
}
