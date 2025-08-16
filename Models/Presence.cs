using System.ComponentModel.DataAnnotations.Schema;

namespace ConstructionApp.Models
{
    public class Presence
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } 
        public float HS { get; set; } // Hours Sunshine   
        public float HR { get; set; } // Hours Rai 
        public float Cost { get; set; }
        // Foreign Key
        public int EmployeeId { get; set; }
        public User Employee { get; set; } = null!;
        public int WorkSiteId { get; set; }
        public WorkSite WorkSite { get; set; } = null!;

    }
}
