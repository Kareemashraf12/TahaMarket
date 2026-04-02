using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TahaMarket.Application.DTOs
{
    public class CreateProductRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public Guid CategoryId { get; set; } 

        [Required]
        public IFormFile Image { get; set; } 
    }
}