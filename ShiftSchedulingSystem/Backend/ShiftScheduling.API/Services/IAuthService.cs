using ShiftScheduling.Core.DTOs;

namespace ShiftScheduling.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
    }
}