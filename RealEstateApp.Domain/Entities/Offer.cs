using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApp.Domain.Entities
{
    public class Offer : BaseEntity
    {
        public int PropertyId { get; set; }
        public string ClientId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column("Date")]
        public DateTime OfferDate { get; set; }
        public OfferStatus Status { get; set; }

        public Property Property { get; set; } = null!;
    }
}