using Microsoft.EntityFrameworkCore;
using ShiftScheduling.Core.DTOs;
using ShiftScheduling.Core.Entities;
using ShiftScheduling.Infrastructure.Data;
using ShiftScheduling.Infrastructure.Repositories;

namespace ShiftScheduling.API.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IRepository<Shift> _shiftRepository;
        private readonly ApplicationDbContext _context;

        public ShiftService(IRepository<Shift> shiftRepository, ApplicationDbContext context)
        {
            _shiftRepository = shiftRepository;
            _context = context;
        }

        public async Task<IEnumerable<ShiftDto>> GetAllShiftsAsync()
        {
            var shifts = await _context.Shifts
                .Include(s => s.User)
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return shifts.Select(s => MapToDto(s));
        }

        public async Task<ShiftDto?> GetShiftByIdAsync(int id)
        {
            var shift = await _context.Shifts
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            return shift != null ? MapToDto(shift) : null;
        }

        public async Task<IEnumerable<ShiftDto>> GetShiftsByUserAsync(int userId)
        {
            var shifts = await _context.Shifts
                .Include(s => s.User)
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return shifts.Select(s => MapToDto(s));
        }

        public async Task<WeeklyScheduleDto> GetWeeklyScheduleAsync(DateTime weekStartDate)
        {
            var weekStart = weekStartDate.Date;
            var weekEnd = weekStart.AddDays(7);
            
            var shifts = await _context.Shifts
                .Include(s => s.User)
                .Where(s => s.ShiftDate >= weekStart && s.ShiftDate < weekEnd)
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var shiftsByDay = shifts
                .GroupBy(s => s.ShiftDate.ToString("yyyy-MM-dd"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(MapToDto).ToList()
                );

            return new WeeklyScheduleDto
            {
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd.AddDays(-1),
                Year = weekStart.Year,
                WeekNumber = GetWeekNumber(weekStart),
                Shifts = shifts.Select(MapToDto).ToList(),
                ShiftsByDay = shiftsByDay
            };
        }

        public async Task<WeeklyScheduleDto> GetUserWeeklyScheduleAsync(int userId, DateTime weekStartDate)
        {
            var weekStart = weekStartDate.Date;
            var weekEnd = weekStart.AddDays(7);
            
            var shifts = await _context.Shifts
                .Include(s => s.User)
                .Where(s => s.UserId == userId && s.ShiftDate >= weekStart && s.ShiftDate < weekEnd)
                .OrderBy(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var shiftsByDay = shifts
                .GroupBy(s => s.ShiftDate.ToString("yyyy-MM-dd"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(MapToDto).ToList()
                );

            return new WeeklyScheduleDto
            {
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd.AddDays(-1),
                Year = weekStart.Year,
                WeekNumber = GetWeekNumber(weekStart),
                Shifts = shifts.Select(MapToDto).ToList(),
                ShiftsByDay = shiftsByDay
            };
        }

        public async Task<ShiftDto> CreateShiftAsync(CreateShiftDto createShiftDto)
        {
            // Check for overlapping shifts
            var overlapping = await _context.Shifts
                .AnyAsync(s => s.UserId == createShiftDto.UserId && 
                              s.ShiftDate == createShiftDto.ShiftDate &&
                              ((s.StartTime <= createShiftDto.StartTime && s.EndTime > createShiftDto.StartTime) ||
                               (s.StartTime < createShiftDto.EndTime && s.EndTime >= createShiftDto.EndTime)));

            if (overlapping)
                throw new InvalidOperationException("Employee already has a shift during this time period");

            var shift = new Shift
            {
                UserId = createShiftDto.UserId,
                ShiftDate = createShiftDto.ShiftDate,
                StartTime = createShiftDto.StartTime,
                EndTime = createShiftDto.EndTime,
                ShiftType = createShiftDto.ShiftType,
                Department = createShiftDto.Department,
                Notes = createShiftDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _shiftRepository.AddAsync(shift);
            
            // Load the user relationship
            await _context.Entry(created).Reference(s => s.User).LoadAsync();
            
            return MapToDto(created);
        }

        public async Task<ShiftDto?> UpdateShiftAsync(UpdateShiftDto updateShiftDto)
        {
            var existing = await _shiftRepository.GetByIdAsync(updateShiftDto.Id);
            if (existing == null)
                return null;

            // Check for overlapping shifts (excluding current shift)
            var overlapping = await _context.Shifts
                .AnyAsync(s => s.Id != updateShiftDto.Id &&
                              s.UserId == updateShiftDto.UserId && 
                              s.ShiftDate == updateShiftDto.ShiftDate &&
                              ((s.StartTime <= updateShiftDto.StartTime && s.EndTime > updateShiftDto.StartTime) ||
                               (s.StartTime < updateShiftDto.EndTime && s.EndTime >= updateShiftDto.EndTime)));

            if (overlapping)
                throw new InvalidOperationException("Employee already has a shift during this time period");

            existing.UserId = updateShiftDto.UserId;
            existing.ShiftDate = updateShiftDto.ShiftDate;
            existing.StartTime = updateShiftDto.StartTime;
            existing.EndTime = updateShiftDto.EndTime;
            existing.ShiftType = updateShiftDto.ShiftType;
            existing.Department = updateShiftDto.Department;
            existing.Notes = updateShiftDto.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _shiftRepository.UpdateAsync(existing);
            
            await _context.Entry(existing).Reference(s => s.User).LoadAsync();
            
            return MapToDto(existing);
        }

        public async Task<bool> DeleteShiftAsync(int id)
        {
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                return false;

            await _shiftRepository.DeleteAsync(shift);
            return true;
        }

        public async Task<IEnumerable<ShiftDto>> GetAvailableShiftsForSwapAsync(int userId)
        {
            // Get shifts that are not involved in pending swap requests
            var userShifts = await _context.Shifts
                .Include(s => s.User)
                .Where(s => s.UserId == userId && s.ShiftDate >= DateTime.Today)
                .ToListAsync();

            var pendingSwapShiftIds = await _context.ShiftSwapRequests
                .Where(r => r.Status == "Pending" && 
                           (r.RequestorId == userId || r.RequestedUserId == userId))
                .Select(r => r.RequestorShiftId)
                .Union(_context.ShiftSwapRequests
                    .Where(r => r.Status == "Pending" && 
                               (r.RequestorId == userId || r.RequestedUserId == userId))
                    .Select(r => r.RequestedShiftId))
                .ToListAsync();

            var availableShifts = userShifts
                .Where(s => !pendingSwapShiftIds.Contains(s.Id))
                .Select(MapToDto)
                .ToList();

            return availableShifts;
        }

        public async Task<bool> BulkCreateShiftsAsync(BulkCreateShiftsDto bulkCreateDto)
        {
            if (bulkCreateDto.OverlapExisting)
            {
                // Delete existing shifts for the same dates and users
                foreach (var shiftDto in bulkCreateDto.Shifts)
                {
                    var existingShifts = await _context.Shifts
                        .Where(s => s.UserId == shiftDto.UserId && s.ShiftDate == shiftDto.ShiftDate)
                        .ToListAsync();
                    
                    _context.Shifts.RemoveRange(existingShifts);
                }
                await _context.SaveChangesAsync();
            }

            var shifts = bulkCreateDto.Shifts.Select(dto => new Shift
            {
                UserId = dto.UserId,
                ShiftDate = dto.ShiftDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                ShiftType = dto.ShiftType,
                Department = dto.Department,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Shifts.AddRangeAsync(shifts);
            await _context.SaveChangesAsync();
            
            return true;
        }

        private ShiftDto MapToDto(Shift shift)
        {
            return new ShiftDto
            {
                Id = shift.Id,
                UserId = shift.UserId,
                UserName = shift.User != null ? $"{shift.User.FirstName} {shift.User.LastName}" : null,
                UserEmail = shift.User?.Email,
                ShiftDate = shift.ShiftDate,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                ShiftType = shift.ShiftType,
                Department = shift.Department,
                Notes = shift.Notes,
                CreatedAt = shift.CreatedAt
            };
        }

        private int GetWeekNumber(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            var weekRule = culture.DateTimeFormat.CalendarWeekRule;
            var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            
            return calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }
    }
}