using System.ComponentModel.DataAnnotations;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class AddRatingRequest
    {
        [Required]
        public Guid TargetId { get; set; }   

        [Required]
        public RatingTargetType TargetType { get; set; }  

        [Range(1, 5)]
        public int Value { get; set; }

        public string? Comment { get; set; }
    }
}