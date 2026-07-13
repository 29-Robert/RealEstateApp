using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Improvements
{
    public class SaveImprovementViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        public string Description { get; set; } = null!;
    }
}