namespace RealEstateApp.Application.ViewModels.PropertyTypes
{
    public class PropertyTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int PropertiesCount { get; set; }
    }
}