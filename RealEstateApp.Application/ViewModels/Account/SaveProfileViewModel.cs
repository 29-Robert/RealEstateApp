using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Account
{
    public class SaveProfileViewModel
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [DataType(DataType.Text)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido.")]
        [DataType(DataType.PhoneNumber)]
        public required string PhoneNumber { get; set; }

        public string? ImagePath { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? File { get; set; }
    }
}