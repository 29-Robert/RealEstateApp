using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public string SenderId { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;
        public int PropertyId { get; set; }
        public string Message { get; set; } = null!;
        public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Property Property { get; set; } = null!;
    }
}