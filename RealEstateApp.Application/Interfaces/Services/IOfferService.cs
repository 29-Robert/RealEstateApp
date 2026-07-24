using RealEstateApp.Application.DTOs.Offer.RealEstateApp.Application.DTOs.Offer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IOfferService
    {
        Task<OfferDto> CreateOfferAsync(SaveOfferDto dto);
        Task<List<OfferDto>> GetByAgentAsync(string agentId);
        Task AcceptOfferAsync(int offerId);
        Task RejectOfferAsync(int offerId);
    }
}
