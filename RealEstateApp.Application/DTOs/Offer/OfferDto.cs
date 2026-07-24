using RealEstateApp.Domain.Enums;
using System;

namespace RealEstateApp.Application.DTOs.Offer
{
    public class OfferDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string ClientId { get; set; } = null!;
        public string? ClientFullName { get; set; }
        public decimal Amount { get; set; }
        public DateTime OfferDate { get; set; }
        public OfferStatus Status { get; set; }
    }
}
