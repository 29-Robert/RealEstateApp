using System;

namespace RealEstateApp.Application.ViewModels.Message
{
    public class AgentConversationViewModel
    {
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string LastMessage { get; set; } = null!;
        public DateTime LastMessageDate { get; set; }
    }
}
