using System;

namespace RealEstateApp.Application.DTOs.Message
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string ClientId { get; set; } = null!;
        public string? ClientName { get; set; }
        public string AgentId { get; set; } = null!;
        public string? AgentName { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsFromClient { get; set; }
    }
}
