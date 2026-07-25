using RealEstateApp.Application.DTOs.Offer;
using RealEstateApp.Application.ViewModels.Offer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IOfferService
    {
        Task<List<OfferDto>> GetClientOffersAsync(int propertyId, string clientId);
        Task<List<OfferDto>> GetOffersByPropertyAsync(int propertyId);
        Task<List<OfferDto>> GetAgentOffersAsync(string agentId);
        Task CreateOfferAsync(SendOfferViewModel vm, string clientId);
        Task AcceptOfferAsync(int offerId, string agentId);
        Task RejectOfferAsync(int offerId, string agentId);
        Task<bool> HasPendingOfferAsync(int propertyId, string clientId);
        Task<bool> HasAcceptedOfferAsync(int propertyId);
        Task<OfferDto> CreateOfferAsync(SaveOfferDto dto);
        Task<List<OfferDto>> GetByAgentAsync(string agentId);
        Task AcceptOfferAsync(int offerId);
        Task RejectOfferAsync(int offerId);
    }
}
