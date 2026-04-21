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
    public class ShiftSwapsController : ControllerBase
    {
        private readonly IShiftSwapService _shiftSwapService;

        public ShiftSwapsController(IShiftSwapService shiftSwapService)
        {
            _shiftSwapService = shiftSwapService;
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAllSwapRequests()
        {
            var requests = await _shiftSwapService.GetAllSwapRequestsAsync();
            return Ok(requests);
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMySwapRequests()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var requests = await _shiftSwapService.GetSwapRequestsForUserAsync(userId);
            return Ok(requests);
        }

        [HttpGet("pending-for-me")]
        public async Task<IActionResult> GetPendingSwapRequestsForMe()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var requests = await _shiftSwapService.GetPendingSwapRequestsForUserAsync(userId);
            return Ok(requests);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSwapRequest([FromBody] CreateShiftSwapRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var request = await _shiftSwapService.CreateSwapRequestAsync(requestDto, userId);
                return Ok(request);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateSwapRequestStatus(int id, [FromBody] UpdateShiftSwapRequestDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
                
                var request = await _shiftSwapService.UpdateSwapRequestStatusAsync(updateDto, userId, userRole);
                
                if (request == null)
                    return NotFound(new { message = "Swap request not found" });

                return Ok(request);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelSwapRequest(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _shiftSwapService.CancelSwapRequestAsync(id, userId);
                
                if (!result)
                    return NotFound(new { message = "Swap request not found" });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}