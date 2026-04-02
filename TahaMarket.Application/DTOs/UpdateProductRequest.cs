using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TahaMarket.Application.DTOs
{
    public class UpdateProductRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public IFormFile? Image { get; set; }
    }
}