using System.ComponentModel.DataAnnotations;

namespace ConstructionApp.ViewModels
{
    public class AttachFileViewModel
    {
        public int Id { get; set; }
        public IFormFile? FormFile { get; set; }
        public string? Type {  get; set; }
        public string? Name {  get; set; }
        [Required]
        public string FileName { get; set; } = string.Empty;
        public string? StoredFilePath { get; set; }
        [Required]
        public DateTime Deadline { get; set; } = DateTime.Now;
        [Required]
        public string Description { get; set; } = string.Empty;
        public string? Author { get; set; }
        public bool Renewed { get; set; } = false;
        public bool Dumped { get; set; } = false;
        public int? TempId { get; set; }
    }
}
