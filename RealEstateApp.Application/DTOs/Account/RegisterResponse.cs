namespace RealEstateApp.Application.DTOs.Account
{
    public class RegisterResponse
    {
        public bool HasError { get; set; }
        public string? ErrorDescription { get; set; }
    }
}