using Microsoft.EntityFrameworkCore;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class OfferRepository(ApplicationDbContext dbContext) : GenericRepository<Offer>(dbContext), IOfferRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public OfferRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> HasPendingOfferAsync(string clientId, int propertyId)
        {
            return await _dbContext.Set<Offer>().AnyAsync(o =>
                o.ClientId == clientId && o.PropertyId == propertyId && o.Status == OfferStatus.Pendiente);
        }

        public async Task<bool> HasAcceptedOfferAsync(int propertyId)
        {
            return await _dbContext.Set<Offer>().AnyAsync(o => o.PropertyId == propertyId && o.Status == OfferStatus.Aceptada);
        }

        public async Task<List<Offer>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.Set<Offer>()
                .Include(o => o.Property)
                .Where(o => o.PropertyId == propertyId)
                .OrderByDescending(o => o.OfferDate)
                .ToListAsync();
        }

        public async Task<List<Offer>> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<Offer>()
                .Include(o => o.Property)
                .Where(o => o.Property.AgentId == agentId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task AcceptOfferWithCascadeAsync(int offerId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var offer = await _dbContext.Set<Offer>()
                    .Include(o => o.Property)
                    .FirstOrDefaultAsync(o => o.Id == offerId);

                if (offer is null) throw new InvalidOperationException("Oferta no encontrada");

                offer.Status = OfferStatus.Aceptada;
                offer.UpdatedAt = DateTime.UtcNow;

                var otherPendingOffers = await _dbContext.Set<Offer>()
                    .Where(o => o.PropertyId == offer.PropertyId && o.Id != offerId && o.Status == OfferStatus.Pendiente)
                    .ToListAsync();

                foreach (var otherOffer in otherPendingOffers)
                {
                    otherOffer.Status = OfferStatus.Rechazada;
                    otherOffer.UpdatedAt = DateTime.UtcNow;
                }

                if (offer.Property != null)
                {
                    offer.Property.Status = PropertyStatus.Vendida.ToString();
                    offer.Property.UpdatedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Offer?> RejectOfferAsync(int offerId)
        {
            var offer = await _dbContext.Set<Offer>().FindAsync(offerId);
            if (offer is null) return null;

            offer.Status = OfferStatus.Rechazada;
            offer.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return offer;
        }
    }
}