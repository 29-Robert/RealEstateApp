using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RealEstateApp.Application.ViewModels.Agent
{
    public class AgentViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PropertyCount { get; set; }
        public string CurrentPhotoPath { get; set; } = string.Empty;
        public IFormFile? NewPhoto { get; set; } // Opcional: conserva la actual si no sube una nueva
    }

}
