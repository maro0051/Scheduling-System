using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftScheduling.API.Services;
using ShiftScheduling.Core.DTOs;
using System.Security.Claims;

namespace ShiftScheduling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAllShifts()
        {
            var shifts = await _shiftService.GetAllShiftsAsync();
            return Ok(shifts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(int id)
        {
            var shift = await _shiftService.GetShiftByIdAsync(id);
            
            if (shift == null)
                return NotFound(new { message = "Shift not found" });

            // Check authorization
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (userRole != "Manager" && shift.UserId != userId)
                return Forbid();

            return Ok(shift);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetShiftsByUser(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (userRole != "Manager" && currentUserId != userId)
                return Forbid();

            var shifts = await _shiftService.GetShiftsByUserAsync(userId);
            return Ok(shifts);
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklySchedule([FromQuery] DateTime? weekStartDate)
        {
            var startDate = weekStartDate ?? GetStartOfWeek(DateTime.Today);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            WeeklyScheduleDto schedule;
            
            if (userRole == "Manager")
            {
                schedule = await _shiftService.GetWeeklyScheduleAsync(startDate);
            }
            else
            {
                schedule = await _shiftService.GetUserWeeklyScheduleAsync(userId, startDate);
            }
            
            return Ok(schedule);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto createShiftDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var shift = await _shiftService.CreateShiftAsync(createShiftDto);
                return CreatedAtAction(nameof(GetShiftById), new { id = shift.Id }, shift);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateShiftDto updateShiftDto)
        {
            if (id != updateShiftDto.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var shift = await _shiftService.UpdateShiftAsync(updateShiftDto);
                
                if (shift == null)
                    return NotFound(new { message = "Shift not found" });

                return Ok(shift);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            var result = await _shiftService.DeleteShiftAsync(id);
            
            if (!result)
                return NotFound(new { message = "Shift not found" });

            return NoContent();
        }

        [HttpGet("available-for-swap")]
        public async Task<IActionResult> GetAvailableShiftsForSwap()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var shifts = await _shiftService.GetAvailableShiftsForSwapAsync(userId);
            return Ok(shifts);
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> BulkCreateShifts([FromBody] BulkCreateShiftsDto bulkCreateDto)
        {
            if (bulkCreateDto.Shifts == null || !bulkCreateDto.Shifts.Any())
                return BadRequest(new { message = "No shifts provided" });

            var result = await _shiftService.BulkCreateShiftsAsync(bulkCreateDto);
            return Ok(new { success = result, message = "Shifts created successfully" });
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }
}