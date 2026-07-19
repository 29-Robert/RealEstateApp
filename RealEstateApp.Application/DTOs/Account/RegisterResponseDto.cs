namespace RealEstateApp.Application.DTOs.Account
{
    public class RegisterResponseDto
    {
        public bool HasError { get; set; }
        public string? ErrorDescription { get; set; }
    }
}