using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities
{
    public class Property : BaseEntity
    {
        public string Code { get; set; } = null!;
        public decimal Price { get; set; }
        public int LandSize { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string AgentId { get; set; } = null!;

        public int PropertyTypeId { get; set; }
        public virtual PropertyType PropertyType { get; set; } = null!;

        public int SaleTypeId { get; set; }
        public virtual SaleType SaleType { get; set; } = null!;

        public virtual ICollection<Improvement> Improvements { get; set; } = [];
    }
}