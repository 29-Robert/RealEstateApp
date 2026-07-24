using RealEstateApp.Domain.Enums;
using System;

namespace RealEstateApp.Application.ViewModels.Offer
{
    public class AgentPropertyOfferGroupViewModel
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public int OfferCount { get; set; }
        public decimal LastOfferAmount { get; set; }
        public OfferStatus LastOfferStatus { get; set; }
        public DateTime LastOfferDate { get; set; }
    }
}
