using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities
{
    public class Message : BaseEntity
    {
        public int PropertyId { get; set; }
        public string ClientId { get; set; } = null!;
        public string AgentId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsFromClient { get; set; }

        // Propiedad de navegación
        public virtual Property Property { get; set; } = null!;
    }
}
