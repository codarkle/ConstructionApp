namespace ConstructionApp.Models
{
    public class Maintenance
    {
        public int Id { get; set; }
        public DateTime DateOut { get; set; }
        public int KmOut { get; set; }
        public DateTime DateIn { get; set; }
        public int KmIn { get; set; }
        public string Driver { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
    }
}
