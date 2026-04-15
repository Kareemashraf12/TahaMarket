namespace TahaMarket.Domain.Entities
{
    public class StoreSection
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        

        public string? ImageUrl { get; set; }

        

        //  Relation
        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}