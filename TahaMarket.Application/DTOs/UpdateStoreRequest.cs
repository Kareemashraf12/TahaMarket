using Microsoft.AspNetCore.Http;

namespace TahaMarket.Application.DTOs
{
    public class UpdateStoreRequest
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public IFormFile? Image { get; set; } // optional
    }
}