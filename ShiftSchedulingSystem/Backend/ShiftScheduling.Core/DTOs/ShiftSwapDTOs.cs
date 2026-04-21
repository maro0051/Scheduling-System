using System.ComponentModel.DataAnnotations;

namespace ShiftScheduling.Core.DTOs
{
    public class ShiftSwapRequestDto
    {
        public int Id { get; set; }
        public int RequestorShiftId { get; set; }
        public int RequestedShiftId { get; set; }
        public int RequestorId { get; set; }
        public string? RequestorName { get; set; }
        public int RequestedUserId { get; set; }
        public string? RequestedUserName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public ShiftDto? RequestorShift { get; set; }
        public ShiftDto? RequestedShift { get; set; }
    }

    public class CreateShiftSwapRequestDto
    {
        [Required]
        public int RequestorShiftId { get; set; }
        
        [Required]
        public int RequestedShiftId { get; set; }
        
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class UpdateShiftSwapRequestDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty; // Approved or Declined
        
        public string? ResponseMessage { get; set; } // Add this property
    }
}