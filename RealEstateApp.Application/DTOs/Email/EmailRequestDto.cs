using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.DTOs.Email
{
    public class EmailRequestDto
    {
        public string? To { get; set; }
        public required string Subject { get; set; }
        public required string HtmlBody { get; set; }
    }
}
