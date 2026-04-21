using ShiftScheduling.Core.DTOs;

namespace ShiftScheduling.API.Services
{
    public interface IShiftService
    {
        Task<IEnumerable<ShiftDto>> GetAllShiftsAsync();
        Task<ShiftDto?> GetShiftByIdAsync(int id);
        Task<IEnumerable<ShiftDto>> GetShiftsByUserAsync(int userId);
        Task<WeeklyScheduleDto> GetWeeklyScheduleAsync(DateTime weekStartDate);
        Task<WeeklyScheduleDto> GetUserWeeklyScheduleAsync(int userId, DateTime weekStartDate);
        Task<ShiftDto> CreateShiftAsync(CreateShiftDto createShiftDto);
        Task<ShiftDto?> UpdateShiftAsync(UpdateShiftDto updateShiftDto);
        Task<bool> DeleteShiftAsync(int id);
        Task<IEnumerable<ShiftDto>> GetAvailableShiftsForSwapAsync(int userId);
        Task<bool> BulkCreateShiftsAsync(BulkCreateShiftsDto bulkCreateDto);
    }
}