using Microsoft.EntityFrameworkCore;
using ShiftScheduling.Core.DTOs;
using ShiftScheduling.Core.Entities;
using ShiftScheduling.Infrastructure.Data;
using ShiftScheduling.Infrastructure.Repositories;

namespace ShiftScheduling.API.Services
{
    public class ShiftSwapService : IShiftSwapService
    {
        private readonly IRepository<ShiftSwapRequest> _swapRequestRepository;
        private readonly IRepository<Shift> _shiftRepository;
        private readonly ApplicationDbContext _context;

        public ShiftSwapService(
            IRepository<ShiftSwapRequest> swapRequestRepository,
            IRepository<Shift> shiftRepository,
            ApplicationDbContext context)
        {
            _swapRequestRepository = swapRequestRepository;
            _shiftRepository = shiftRepository;
            _context = context;
        }

        public async Task<IEnumerable<ShiftSwapRequestDto>> GetAllSwapRequestsAsync()
        {
            var requests = await _context.ShiftSwapRequests
                .Include(r => r.RequestorShift)
                    .ThenInclude(s => s.User)
                .Include(r => r.RequestedShift)
                    .ThenInclude(s => s.User)
                .Include(r => r.Requestor)
                .Include(r => r.RequestedUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }

        public async Task<IEnumerable<ShiftSwapRequestDto>> GetSwapRequestsForUserAsync(int userId)
        {
            var requests = await _context.ShiftSwapRequests
                .Include(r => r.RequestorShift)
                    .ThenInclude(s => s.User)
                .Include(r => r.RequestedShift)
                    .ThenInclude(s => s.User)
                .Include(r => r.Requestor)
                .Include(r => r.RequestedUser)
                .Where(r => r.RequestorId == userId || r.RequestedUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }

        public async Task<IEnumerable<ShiftSwapRequestDto>> GetPendingSwapRequestsForUserAsync(int userId)
        {
            var requests = await _context.ShiftSwapRequests
                .Include(r => r.RequestorShift)
                    .ThenInclude(s => s.User)
                .Include(r => r.RequestedShift)
                    .ThenInclude(s => s.User)
                .Include(r => r.Requestor)
                .Include(r => r.RequestedUser)
                .Where(r => r.RequestedUserId == userId && r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(MapToDto);
        }

        public async Task<ShiftSwapRequestDto?> CreateSwapRequestAsync(CreateShiftSwapRequestDto requestDto, int requestorId)
        {
            // Validate shifts exist
            var requestorShift = await _shiftRepository.GetByIdAsync(requestDto.RequestorShiftId);
            var requestedShift = await _shiftRepository.GetByIdAsync(requestDto.RequestedShiftId);

            if (requestorShift == null || requestedShift == null)
                throw new ArgumentException("Invalid shift IDs");

            // Validate requestor owns the requestor shift
            if (requestorShift.UserId != requestorId)
                throw new UnauthorizedAccessException("You can only request swaps for your own shifts");

            // Check if there's already a pending request for these shifts
            var existingRequest = await _context.ShiftSwapRequests
                .AnyAsync(r => r.Status == "Pending" && 
                              ((r.RequestorShiftId == requestDto.RequestorShiftId && 
                                r.RequestedShiftId == requestDto.RequestedShiftId) ||
                               (r.RequestorShiftId == requestDto.RequestedShiftId && 
                                r.RequestedShiftId == requestDto.RequestorShiftId)));

            if (existingRequest)
                throw new InvalidOperationException("A swap request already exists for these shifts");

            var swapRequest = new ShiftSwapRequest
            {
                RequestorShiftId = requestDto.RequestorShiftId,
                RequestedShiftId = requestDto.RequestedShiftId,
                RequestorId = requestorId,
                RequestedUserId = requestedShift.UserId,
                Reason = requestDto.Reason,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _swapRequestRepository.AddAsync(swapRequest);
            
            // Load all navigation properties
            await _context.Entry(created)
                .Reference(r => r.RequestorShift)
                .Query()
                .Include(s => s.User)
                .LoadAsync();
            
            await _context.Entry(created)
                .Reference(r => r.RequestedShift)
                .Query()
                .Include(s => s.User)
                .LoadAsync();
            
            await _context.Entry(created).Reference(r => r.Requestor).LoadAsync();
            await _context.Entry(created).Reference(r => r.RequestedUser).LoadAsync();

            return MapToDto(created);
        }

        public async Task<ShiftSwapRequestDto?> UpdateSwapRequestStatusAsync(UpdateShiftSwapRequestDto updateDto, int currentUserId, string userRole)
        {
            var request = await _context.ShiftSwapRequests
                .Include(r => r.RequestorShift)
                .Include(r => r.RequestedShift)
                .Include(r => r.Requestor)
                .Include(r => r.RequestedUser)
                .FirstOrDefaultAsync(r => r.Id == updateDto.Id);

            if (request == null)
                return null;

            // Check authorization
            if (userRole != "Manager" && request.RequestedUserId != currentUserId)
                throw new UnauthorizedAccessException("You don't have permission to update this request");

            if (request.Status != "Pending")
                throw new InvalidOperationException($"Cannot update request with status: {request.Status}");

            request.Status = updateDto.Status;
            request.RespondedAt = DateTime.UtcNow;

            // If approved, swap the shifts
            if (updateDto.Status == "Approved")
            {
                // Swap user IDs between shifts
                var tempUserId = request.RequestorShift.UserId;
                request.RequestorShift.UserId = request.RequestedShift.UserId;
                request.RequestedShift.UserId = tempUserId;
                
                request.RequestorShift.UpdatedAt = DateTime.UtcNow;
                request.RequestedShift.UpdatedAt = DateTime.UtcNow;
                
                await _shiftRepository.UpdateAsync(request.RequestorShift);
                await _shiftRepository.UpdateAsync(request.RequestedShift);
            }

            await _swapRequestRepository.UpdateAsync(request);
            
            return MapToDto(request);
        }

        public async Task<bool> CancelSwapRequestAsync(int requestId, int userId)
        {
            var request = await _swapRequestRepository.GetByIdAsync(requestId);
            
            if (request == null)
                return false;
            
            if (request.RequestorId != userId)
                throw new UnauthorizedAccessException("Only the requestor can cancel the request");
            
            if (request.Status != "Pending")
                throw new InvalidOperationException("Cannot cancel a request that is no longer pending");
            
            request.Status = "Cancelled";
            request.RespondedAt = DateTime.UtcNow;
            
            await _swapRequestRepository.UpdateAsync(request);
            return true;
        }

        private ShiftSwapRequestDto MapToDto(ShiftSwapRequest request)
        {
            return new ShiftSwapRequestDto
            {
                Id = request.Id,
                RequestorShiftId = request.RequestorShiftId,
                RequestedShiftId = request.RequestedShiftId,
                RequestorId = request.RequestorId,
                RequestorName = request.Requestor != null ? $"{request.Requestor.FirstName} {request.Requestor.LastName}" : null,
                RequestedUserId = request.RequestedUserId,
                RequestedUserName = request.RequestedUser != null ? $"{request.RequestedUser.FirstName} {request.RequestedUser.LastName}" : null,
                Status = request.Status,
                Reason = request.Reason,
                CreatedAt = request.CreatedAt,
                RespondedAt = request.RespondedAt,
                RequestorShift = request.RequestorShift != null ? MapShiftToDto(request.RequestorShift) : null,
                RequestedShift = request.RequestedShift != null ? MapShiftToDto(request.RequestedShift) : null
            };
        }

        private ShiftDto MapShiftToDto(Shift shift)
        {
            return new ShiftDto
            {
                Id = shift.Id,
                UserId = shift.UserId,
                UserName = shift.User != null ? $"{shift.User.FirstName} {shift.User.LastName}" : null,
                ShiftDate = shift.ShiftDate,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                ShiftType = shift.ShiftType,
                Department = shift.Department,
                Notes = shift.Notes
            };
        }
    }
}