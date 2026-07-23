using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RealEstateApp.Application.ViewModels.Users
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe introducir una dirección de correo válida.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "El nombre de usuario debe tener entre 4 y 20 caracteres.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Phone(ErrorMessage = "Debe introducir un número de teléfono válido.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe contener exactamente 10 dígitos numéricos.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar un tipo de usuario (Rol).")]
        public string UserType { get; set; } = null!; 

        public string? CardId { get; set; }
        public IFormFile? Photo { get; set; }
    }
}