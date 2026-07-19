namespace RealEstateApp.Application.DTOs.Account
{
    public  class AgentResponseDto
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool IsActive { get; set; }
        public int PropertyCount { get; set; } 
    }
}
