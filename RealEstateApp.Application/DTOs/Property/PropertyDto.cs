using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.DTOs.Property
{
    public class PropertyDto
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double Size { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MainImagePath { get; set; } = string.Empty;
        public string PropertyTypeName { get; set; } = string.Empty;
        public string SaleTypeName { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
    }
}
