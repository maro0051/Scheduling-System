using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftScheduling.API.Services;
using ShiftScheduling.Core.DTOs;

namespace ShiftScheduling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(result);
        }

        // DISABLE SELF-REGISTRATION - Comment out or remove this endpoint
        /*
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);
            
            if (result == null)
                return BadRequest(new { message = "User with this email already exists" });

            return Ok(result);
        }
        */

        [Authorize]
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var profile = await _authService.GetUserProfileAsync(userId);
            
            if (profile == null)
                return NotFound(new { message = "User not found" });

            return Ok(profile);
        }
    }
}