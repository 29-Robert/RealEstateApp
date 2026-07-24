using AutoMapper;
using RealEstateApp.Application.DTOs.Offer;
using RealEstateApp.Application.DTOs.Offer.RealEstateApp.Application.DTOs.Offer;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Services
{
   
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IMapper _mapper;

        public OfferService(IOfferRepository offerRepository, IMapper mapper)
        {
            _offerRepository = offerRepository;
            _mapper = mapper;
        }

        public async Task<OfferDto> CreateOfferAsync(SaveOfferDto dto)
        {
            // Bloqueos minuciosos
            if (await _offerRepository.HasPendingOfferAsync(dto.ClientId, dto.PropertyId))
                throw new InvalidOperationException("Ya tienes una oferta pendiente en esta propiedad");

            if (await _offerRepository.HasAcceptedOfferAsync(dto.PropertyId))
                throw new InvalidOperationException("Esta propiedad ya tiene una oferta aceptada");

            var entity = _mapper.Map<Offer>(dto);
            var created = await _offerRepository.AddAsync(entity);

            return _mapper.Map<OfferDto>(created);
        }

        public async Task<List<OfferDto>> GetByAgentAsync(string agentId)
        {
            var entities = await _offerRepository.GetByAgentIdAsync(agentId);
            return _mapper.Map<List<OfferDto>>(entities);
        }

       
        // oferta ,Aceptada, demas ofertas Pendientes de la propiedad
        // Rechazada, propiedad y Vendida.
        
        public async Task AcceptOfferAsync(int offerId) => await _offerRepository.AcceptOfferWithCascadeAsync(offerId);

        public async Task RejectOfferAsync(int offerId) => await _offerRepository.RejectOfferAsync(offerId);
    }
}