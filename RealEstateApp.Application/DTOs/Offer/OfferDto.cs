using RealEstateApp.Domain.Enums;
using System;

namespace RealEstateApp.Application.DTOs.Offer
{
    public class OfferDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientFullName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime OfferDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public OfferStatus Status { get; set; }
    }
}
