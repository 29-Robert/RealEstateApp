using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using System;

namespace RealEstateApp.Domain.Entities
{
    public class Offer : BaseEntity
    {
        public int PropertyId { get; set; }
        public string ClientId { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime OfferDate { get; set; } = DateTime.Now;
        public OfferStatus Status { get; set; } = OfferStatus.Pendiente;

        // Propiedad de navegación
        public virtual Property Property { get; set; } = null!;
    }
}