using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TahaMarket.Application.DTOs
{


    public class CreateStoreRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public decimal MinimumOrderAmount { get; set; }

        [Required]
        public IFormFile Image { get; set; }

        [Required]
        public Guid StoreSectionId { get; set; }

        public string? Description { get; set; }

        
        public TimeSpan TimeOpen { get; set; }

        // 🔥 NEW
        public TimeSpan TimeClose { get; set; }
    }
}
