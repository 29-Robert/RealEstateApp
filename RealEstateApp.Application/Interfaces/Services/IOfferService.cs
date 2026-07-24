using RealEstateApp.Application.DTOs.Offer;
using RealEstateApp.Application.ViewModels.Offer;
using System.Collections.Generic;
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
    }
}
