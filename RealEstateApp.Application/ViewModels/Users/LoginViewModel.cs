using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Users
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Debe introducir un correo válido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}