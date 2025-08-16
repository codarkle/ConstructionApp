namespace ConstructionApp.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public DateTime DateInsurance { get; set; }
        public DateTime DateRevision { get; set; }
        public DateTime DateMaintenance { get; set; }
        public string? Picture { get; set; }
        public ICollection<AttachFile> AttachFiles { get; set; } = new List<AttachFile>();
    } 
}