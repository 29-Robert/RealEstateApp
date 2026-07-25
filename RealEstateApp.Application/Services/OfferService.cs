using AutoMapper;
using RealEstateApp.Application.DTOs.Offer;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Offer;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Application.Services
{
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public OfferService(
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository,
            IAccountService accountService,
            IMapper mapper)
        {
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;
            _accountService = accountService;
            _mapper = mapper;
        }

        public async Task<List<OfferDto>> GetClientOffersAsync(int propertyId, string clientId)
        {
            var offers = await _offerRepository.GetAllAsync();
            var filtered = offers
                .Where(o => o.PropertyId == propertyId && o.ClientId == clientId)
                .OrderByDescending(o => o.OfferDate)
                .ToList();

            var client = await _accountService.GetUserByIdAsync(clientId);
            string clientName = client != null ? $"{client.FirstName} {client.LastName}" : "Cliente";

            var dtos = _mapper.Map<List<OfferDto>>(filtered);
            foreach (var dto in dtos)
            {
                dto.ClientFullName = clientName;
            }
            return dtos;
        }

        public async Task<List<OfferDto>> GetOffersByPropertyAsync(int propertyId)
        {
            var offers = await _offerRepository.GetAllAsync();
            var filtered = offers
                .Where(o => o.PropertyId == propertyId)
                .OrderByDescending(o => o.OfferDate)
                .ToList();

            var dtos = _mapper.Map<List<OfferDto>>(filtered);
            var cache = new Dictionary<string, string>();
            foreach (var dto in dtos)
            {
                if (!cache.ContainsKey(dto.ClientId))
                {
                    var client = await _accountService.GetUserByIdAsync(dto.ClientId);
                    cache[dto.ClientId] = client != null ? $"{client.FirstName} {client.LastName}" : "Cliente";
                }
                dto.ClientFullName = cache[dto.ClientId];
            }
            return dtos;
        }

        public async Task<List<OfferDto>> GetAgentOffersAsync(string agentId)
        {
            return await GetByAgentAsync(agentId);
        }

        public async Task<List<OfferDto>> GetByAgentAsync(string agentId)
        {
            var agentProperties = await _propertyRepository.GetByAgentIdAsync(agentId, false);
            var propertyIds = agentProperties.Select(p => p.Id).ToList();

            var offers = await _offerRepository.GetAllAsync();
            var filtered = offers
                .Where(o => propertyIds.Contains(o.PropertyId))
                .OrderByDescending(o => o.OfferDate)
                .ToList();

            var dtos = _mapper.Map<List<OfferDto>>(filtered);
            var cache = new Dictionary<string, string>();
            foreach (var dto in dtos)
            {
                if (!cache.ContainsKey(dto.ClientId))
                {
                    var client = await _accountService.GetUserByIdAsync(dto.ClientId);
                    cache[dto.ClientId] = client != null ? $"{client.FirstName} {client.LastName}" : "Cliente";
                }
                dto.ClientFullName = cache[dto.ClientId];
            }
            return dtos;
        }

        public async Task CreateOfferAsync(SendOfferViewModel vm, string clientId)
        {
            var offer = new Offer
            {
                PropertyId = vm.PropertyId,
                ClientId = clientId,
                Amount = vm.Amount,
                OfferDate = DateTime.Now,
                Status = OfferStatus.Pendiente
            };

            await _offerRepository.AddAsync(offer);
        }

        public async Task<OfferDto> CreateOfferAsync(SaveOfferDto dto)
        {
            if (await _offerRepository.HasPendingOfferAsync(dto.ClientId, dto.PropertyId))
                throw new InvalidOperationException("Ya tienes una oferta pendiente en esta propiedad");

            if (await _offerRepository.HasAcceptedOfferAsync(dto.PropertyId))
                throw new InvalidOperationException("Esta propiedad ya tiene una oferta aceptada");

            var entity = _mapper.Map<Offer>(dto);
            var created = await _offerRepository.AddAsync(entity);
            return _mapper.Map<OfferDto>(created);
        }

        public async Task AcceptOfferAsync(int offerId, string agentId)
        {
            await AcceptOfferAsync(offerId);
        }

        public async Task AcceptOfferAsync(int offerId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null) return;

            offer.Status = OfferStatus.Aceptada;
            await _offerRepository.UpdateAsync(offer);

            // Cambiar estado de la propiedad a Vendida
            var property = await _propertyRepository.GetByIdAsync(offer.PropertyId);
            if (property != null)
            {
                property.PropertyStatus = PropertyStatusEnum.Vendida;
                property.Status = PropertyStatusEnum.Vendida.ToString();
                await _propertyRepository.UpdateAsync(property);
            }

            // Rechazar el resto de las ofertas pendientes para esa propiedad
            var allOffers = await _offerRepository.GetAllAsync();
            var pendingOffers = allOffers
                .Where(o => o.PropertyId == offer.PropertyId && o.Id != offer.Id && o.Status == OfferStatus.Pendiente)
                .ToList();

            foreach (var pending in pendingOffers)
            {
                pending.Status = OfferStatus.Rechazada;
                await _offerRepository.UpdateAsync(pending);
            }
        }

        public async Task RejectOfferAsync(int offerId, string agentId)
        {
            await RejectOfferAsync(offerId);
        }

        public async Task RejectOfferAsync(int offerId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null) return;

            offer.Status = OfferStatus.Rechazada;
            await _offerRepository.UpdateAsync(offer);
        }

        public async Task<bool> HasPendingOfferAsync(int propertyId, string clientId)
        {
            var offers = await _offerRepository.GetAllAsync();
            return offers.Any(o => o.PropertyId == propertyId && o.ClientId == clientId && o.Status == OfferStatus.Pendiente);
        }

        public async Task<bool> HasAcceptedOfferAsync(int propertyId)
        {
            var offers = await _offerRepository.GetAllAsync();
            return offers.Any(o => o.PropertyId == propertyId && o.Status == OfferStatus.Aceptada);
        }
    }
}
