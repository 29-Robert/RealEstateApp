namespace RealEstateApp.Application.DTOs.Account
{
    public class AdminDashboardResponseDto
    {
        public int AgentesActivos { get; set; }
        public int AgentesInactivos { get; set; }
        public int ClientesActivos { get; set; }
        public int ClientesInactivos { get; set; }
        public int DesarroladoresActivos { get; set; }
        public int DesarroladoresInactivos { get; set; }
        public int AdministradoresActivos { get; set; }
        public int AdministradoresInactivos { get; set; }

        public int AvailableProperties { get; set; }
        public int SoldProperties { get; set; }
    }
}