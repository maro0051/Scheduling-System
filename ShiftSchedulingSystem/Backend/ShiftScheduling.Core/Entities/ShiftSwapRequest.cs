using System.ComponentModel.DataAnnotations;

namespace ShiftScheduling.Core.Entities
{
    public class ShiftSwapRequest
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int RequestorShiftId { get; set; } // The shift the requestor wants to give away
        
        [Required]
        public int RequestedShiftId { get; set; } // The shift the requestor wants to take
        
        [Required]
        public int RequestorId { get; set; } // Who is requesting the swap
        
        [Required]
        public int RequestedUserId { get; set; } // Who is being asked to swap
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Declined, Cancelled
        
        [MaxLength(500)]
        public string? Reason { get; set; } // Why they want to swap
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? RespondedAt { get; set; } // When the request was responded to
        
        // Navigation properties
        public virtual Shift RequestorShift { get; set; } = null!;
        public virtual Shift RequestedShift { get; set; } = null!;
        public virtual User Requestor { get; set; } = null!;
        public virtual User RequestedUser { get; set; } = null!;
    }
}