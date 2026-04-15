using Microsoft.AspNetCore.Http;

namespace TahaMarket.Application.DTOs
{
    public class UpdateStoreRequest
    {
        // =========================
        // BASIC INFO
        // =========================
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        // =========================
        // DESCRIPTION
        // =========================
        public string? Description { get; set; }

        // =========================
        // WORKING HOURS
        // =========================
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        // =========================
        // IMAGE
        // =========================
        public IFormFile? Image { get; set; }
    }
}