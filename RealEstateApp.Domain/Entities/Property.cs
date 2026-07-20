using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Domain.Entities
{
    public class Property : BaseEntity
    {
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
        public int LandSize { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string AgentId { get; set; } = null!;

        public int PropertyTypeId { get; set; }
        public virtual PropertyType PropertyType { get; set; } = null!;
        public PropertyStatusEnum PropertyStatus { get; set; } = PropertyStatusEnum.Disponible;

        public int SaleTypeId { get; set; }
        public virtual SaleType SaleType { get; set; } = null!;

        public virtual ICollection<Improvement> Improvements { get; set; } = [];
        public virtual ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        //public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}