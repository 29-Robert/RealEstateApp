using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities
{
    public class Favorite : BaseEntity
    {
        public string ClientId { get; set; } = null!;

        public int PropertyId { get; set; }

        public Property Property { get; set; } = null!;

    }
}