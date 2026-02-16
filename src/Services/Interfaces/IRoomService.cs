using RoomBooking.Api.DTOs.Room;
using RoomBooking.Api.DTOs.Common;

namespace RoomBooking.Api.Services.Interfaces;

public interface IRoomService
{
    Task<RoomResponse> CreateAsync(CreateRoomRequest request);
    Task<RoomResponse?> GetByIdAsync(Guid id);
    Task<PagedResult<RoomResponse>> GetAllAsync(int page, int pageSize);
    Task<RoomResponse?> UpdateAsync(Guid id, UpdateRoomRequest request);
    Task<bool> DeleteAsync(Guid id);
}
