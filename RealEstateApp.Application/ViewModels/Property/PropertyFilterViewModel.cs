using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Application.ViewModels.Property
{
    public class PropertyFilterViewModel
    {
        public int? PropertyTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Rooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Code { get; set; } // busqueda exacta por codigo
    }
}
