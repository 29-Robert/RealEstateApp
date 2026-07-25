using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.DTOs.Offer
{
    public class SaveOfferDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "La oferta debe ser mayor a 0 DOP")]
        public decimal Amount { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string ClientId { get; set; } = string.Empty;
    }
}
