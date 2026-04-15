using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TahaMarket.Application.DTOs
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
        public string? Variants { get; set; }
    }
}