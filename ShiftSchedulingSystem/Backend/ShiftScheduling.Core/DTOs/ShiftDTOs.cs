using System.ComponentModel.DataAnnotations;

namespace ShiftScheduling.Core.DTOs
{
    public class ShiftDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ShiftType { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateShiftDto
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime ShiftDate { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        [Required]
        public string ShiftType { get; set; } = string.Empty;
        
        public string? Department { get; set; }
        
        public string? Notes { get; set; }
    }

    public class UpdateShiftDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime ShiftDate { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        [Required]
        public string ShiftType { get; set; } = string.Empty;
        
        public string? Department { get; set; }
        
        public string? Notes { get; set; }
    }

    public class WeeklyScheduleDto
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public List<ShiftDto> Shifts { get; set; } = new();
        public Dictionary<string, List<ShiftDto>> ShiftsByDay { get; set; } = new();
    }
    
    public class BulkCreateShiftsDto
    {
        public List<CreateShiftDto> Shifts { get; set; } = new();
        public bool OverlapExisting { get; set; } = false;
    }
}