using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.DTOs.Property
{
    public class PropertyDetailDto : PropertyDto
    {
        public string Description { get; set; } = string.Empty;
        public List<string> ImagePaths { get; set; } = new();
        public List<string> Improvements { get; set; } = new();
        public string AgentFullName { get; set; } = string.Empty;
        public string AgentPhone { get; set; } = string.Empty;
        public string AgentPhotoPath { get; set; } = string.Empty;
    }
}
