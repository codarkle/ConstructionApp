namespace ConstructionApp.Models
{
    public class AttachFile
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFilePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Author { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool Renewed { get; set; } = false;
        public int? UserId { get; set; }
        public User? User { get; set; }
        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
        public int? WorkSiteId { get; set; }
        public WorkSite? WorkSite { get; set; }
    }
}