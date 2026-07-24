using Microsoft.EntityFrameworkCore;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{

    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        public OfferRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> HasPendingOfferAsync(string clientId, int propertyId)
        {
            return await _entity.AnyAsync(o =>
                o.ClientId == clientId && o.PropertyId == propertyId && o.Status == OfferStatusEnum.Pendiente);
        }

        public async Task<bool> HasAcceptedOfferAsync(int propertyId)
        {
            return await _entity.AnyAsync(o => o.PropertyId == propertyId && o.Status == OfferStatusEnum.Aceptada);
        }

        public async Task<List<Offer>> GetByPropertyIdAsync(int propertyId)
        {
            return await _entity.Include(o => o.Property)
                .Where(o => o.PropertyId == propertyId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Offer>> GetByAgentIdAsync(string agentId)
        {
            return await _entity.Include(o => o.Property)
                .Where(o => o.Property.AgentId == agentId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

   
        // Todo dentro de una transaccion para garantizar consistencia.

        public async Task AcceptOfferWithCascadeAsync(int offerId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var offer = await _entity.Include(o => o.Property).FirstOrDefaultAsync(o => o.Id == offerId);
                if (offer is null) throw new InvalidOperationException("Oferta no encontrada");

                offer.Status = OfferStatusEnum.Aceptada;
                offer.UpdatedAt = DateTime.Now;

                var otherPendingOffers = await _entity
                    .Where(o => o.PropertyId == offer.PropertyId && o.Id != offerId && o.Status == OfferStatusEnum.Pendiente)
                    .ToListAsync();

                foreach (var otherOffer in otherPendingOffers)
                {
                    otherOffer.Status = OfferStatusEnum.Rechazada;
                    otherOffer.UpdatedAt = DateTime.Now;
                }

                offer.Property.Status = PropertyStatusEnum.Vendida;
                offer.Property.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
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
            var offer = await _entity.FindAsync(offerId);
            if (offer is null) return null;

            offer.Status = OfferStatusEnum.Rechazada;
            offer.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return offer;
        }
    }
}