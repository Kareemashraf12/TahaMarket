using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Domain.Entities
{
    public class AddOnGroup
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        // Either Product OR Store
        public Guid? ProductId { get; set; }
        public Product Product { get; set; }

        public Guid? StoreId { get; set; }
        public Store Store { get; set; }

        public bool IsActive { get; set; } = true;

        public List<AddOnOption> Options { get; set; } = new();
    }
}
