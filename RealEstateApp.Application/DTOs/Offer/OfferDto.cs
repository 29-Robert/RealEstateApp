using RealEstateApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.DTOs.Offer
{
    namespace RealEstateApp.Application.DTOs.Offer
    {
    public class OfferDto
    {
        public int Id { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public string ClientId { get; set; } = null!;
        public string? ClientFullName { get; set; }
        public decimal Amount { get; set; }
        public DateTime OfferDate { get; set; }
        public OfferStatus Status { get; set; }
            public string PropertyTitle { get; set; } = string.Empty;
            public string ClientId { get; set; } = string.Empty;
            public string ClientFullName { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}
