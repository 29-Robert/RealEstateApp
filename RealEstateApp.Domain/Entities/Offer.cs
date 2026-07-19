using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Domain.Entities
{
    public class Offer : BaseEntity
    {
        public string ClientId { get; set; } = null!;
        public int PropertyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public EstadoOferta Status { get; set; }

        public Property Property { get; set; } = null!; 
    }
}