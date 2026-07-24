using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApp.Domain.Entities
{
    public class Offer : BaseEntity
    {
        public int PropertyId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; }
        public EstadoOferta Status { get; set; }

        public Property Property { get; set; } = null!;
    }
}