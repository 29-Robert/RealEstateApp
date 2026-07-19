using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.DTOs.Property
{
    public class SavePropertyDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Price { get; set; }

        [Range(1, double.MaxValue)]
        public double Size { get; set; }

        [Range(0, 50)]
        public int Rooms { get; set; }

        [Range(0, 50)]
        public int Bathrooms { get; set; }

        [Required]
        public int PropertyTypeId { get; set; }

        [Required]
        public int SaleTypeId { get; set; }

        public List<int> ImprovementIds { get; set; } = new();

       
        public List<string> NewImagesBase64 { get; set; } = new();

        
        public List<int> ExistingImageIds { get; set; } = new();

        public string AgentId { get; set; } = string.Empty;

     }

 }
