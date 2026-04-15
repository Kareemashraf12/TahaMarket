namespace TahaMarket.Application.DTOs
{
    public class StoreDetailsResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public decimal MinimumOrderAmount { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string ImageUrl { get; set; }

        public string? Description { get; set; }

        public TimeSpan TimeOpen { get; set; }

        public TimeSpan TimeClose { get; set; }

        public double AverageRating { get; set; }

        public int RatingsCount { get; set; }

        public Guid StoreSectionId { get; set; }
    }
}