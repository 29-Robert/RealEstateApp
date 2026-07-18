using RealEstateApp.Application.DTOs.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.DTOs.Agent
{
    public class AgentDetailDto : AgentDto
    {
        public string Phone { get; set; } = string.Empty;
        public List<PropertyDto> AvailableProperties { get; set; } = new();
    }
}
