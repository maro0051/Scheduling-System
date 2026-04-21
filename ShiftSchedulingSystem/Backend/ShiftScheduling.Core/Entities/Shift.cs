using System.ComponentModel.DataAnnotations;

namespace ShiftScheduling.Core.Entities
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; } // Foreign key to User
        
        [Required]
        public DateTime ShiftDate { get; set; } // The date of the shift
        
        [Required]
        public TimeSpan StartTime { get; set; } // When shift starts (e.g., 09:00)
        
        [Required]
        public TimeSpan EndTime { get; set; } // When shift ends (e.g., 17:00)
        
        [Required]
        [MaxLength(50)]
        public string ShiftType { get; set; } = string.Empty; // Morning, Afternoon, Night
        
        [MaxLength(50)]
        public string? Department { get; set; } // Which department this shift belongs to
        
        [MaxLength(500)]
        public string? Notes { get; set; } // Any additional information
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property - each shift belongs to one user
        public virtual User User { get; set; } = null!;
    }
}