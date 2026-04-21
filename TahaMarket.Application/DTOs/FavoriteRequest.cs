using TahaMarket.Domain.Enums;

namespace TahaMarket.Application.DTOs
{
    public class FavoriteRequest
    {
        public Guid TargetId { get; set; }
        public FavoriteType Type { get; set; } // Product = 1, Store = 2
    }
}