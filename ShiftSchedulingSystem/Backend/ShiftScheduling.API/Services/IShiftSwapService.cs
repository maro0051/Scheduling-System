using ShiftScheduling.Core.DTOs;

namespace ShiftScheduling.API.Services
{
    public interface IShiftSwapService
    {
        Task<IEnumerable<ShiftSwapRequestDto>> GetAllSwapRequestsAsync();
        Task<IEnumerable<ShiftSwapRequestDto>> GetSwapRequestsForUserAsync(int userId);
        Task<IEnumerable<ShiftSwapRequestDto>> GetPendingSwapRequestsForUserAsync(int userId);
        Task<ShiftSwapRequestDto?> CreateSwapRequestAsync(CreateShiftSwapRequestDto requestDto, int requestorId);
        Task<ShiftSwapRequestDto?> UpdateSwapRequestStatusAsync(UpdateShiftSwapRequestDto updateDto, int currentUserId, string userRole);
        Task<bool> CancelSwapRequestAsync(int requestId, int userId);
    }
}