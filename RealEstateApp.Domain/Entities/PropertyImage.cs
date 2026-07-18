using RealEstateApp.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Domain.Entities
{
    public class PropertyImage : BaseEntity
    {
        public string ImagePath { get; set; } = string.Empty;
        public bool IsMain { get; set; } 

        public int PropertyId { get; set; }
        public virtual Property Property { get; set; } = null!;
    }
}
