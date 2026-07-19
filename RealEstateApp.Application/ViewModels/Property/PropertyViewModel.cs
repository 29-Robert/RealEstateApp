using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.ViewModels.Property
{
    public class PropertyViewModel
    {
        public int Id { get; set; }
        public int Code { get; set; }

        [Required] public string Title { get; set; } = string.Empty;
        [Required] public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Price { get; set; }

        public double Size { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un tipo de propiedad")]
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de venta")]
        public int SaleTypeId { get; set; }

        public List<int> SelectedImprovementIds { get; set; } = new();

        public List<IFormFile>? Images { get; set; }
        public List<string> ExistingImagePaths { get; set; } = new();

        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> PropertyTypes { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> SaleTypes { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Improvements { get; set; } = new();
    }
}
