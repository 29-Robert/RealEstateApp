using RealEstateApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.Interfaces.Repositories
{
    public interface IOfferRepository : IGenericRepository<Offer>
    {
        Task<bool> HasPendingOfferAsync(string clientId, int propertyId);
        Task<bool> HasAcceptedOfferAsync(int propertyId);
        Task<List<Offer>> GetByPropertyIdAsync(int propertyId);
        Task<List<Offer>> GetByAgentIdAsync(string agentId);
        Task AcceptOfferWithCascadeAsync(int offerId);
        Task<Offer?> RejectOfferAsync(int offerId);
    }
}
