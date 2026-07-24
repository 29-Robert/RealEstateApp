using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Message
{
    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "El id de propiedad es requerido.")]
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "El id de agente es requerido.")]
        public string AgentId { get; set; } = null!;

        [Required(ErrorMessage = "El contenido del mensaje es requerido.")]
        [StringLength(1000, ErrorMessage = "El mensaje no puede exceder los 1000 caracteres.")]
        public string Content { get; set; } = null!;
    }
}
