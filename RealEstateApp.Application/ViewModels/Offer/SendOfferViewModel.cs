using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Offer
{
    public class SendOfferViewModel
    {
        [Required(ErrorMessage = "El id de propiedad es requerido.")]
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto ofertado debe ser mayor a cero.")]
        public decimal Amount { get; set; }
    }
}
