using RealEstateApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.Interfaces.Repositories
{
    public interface IPropertyImageRepository : IGenericRepository<PropertyImage>
    {
        Task<List<PropertyImage>> GetByPropertyIdAsync(int propertyId);
        Task<int> CountByPropertyIdAsync(int propertyId);
    }
}
