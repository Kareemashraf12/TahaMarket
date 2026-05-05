using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.DTOs
{
    public class CreateAddOnGroupWithOptionsRequest
    {
        public string Name { get; set; }

        public Guid? ProductId { get; set; }
        public Guid? StoreId { get; set; }

        public List<AddOnOptionCreateDto> Options { get; set; } = new();
    }
}
