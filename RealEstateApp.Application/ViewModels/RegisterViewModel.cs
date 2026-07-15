using System.ComponentModel.DataAnnotations;


namespace RealEstateApp.Application.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = null!;
    }
}
