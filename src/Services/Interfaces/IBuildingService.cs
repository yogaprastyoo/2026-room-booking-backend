using RoomBooking.Api.DTOs.Building;
using RoomBooking.Api.DTOs.Common;

namespace RoomBooking.Api.Services.Interfaces;

public interface IBuildingService
{
    Task<BuildingResponse> CreateAsync(CreateBuildingRequest request);
    Task<BuildingResponse?> GetByIdAsync(Guid id);
    Task<PagedResult<BuildingResponse>> GetAllAsync(int page, int pageSize);
    Task<BuildingResponse?> UpdateAsync(Guid id, UpdateBuildingRequest request);
    Task<bool> DeleteAsync(Guid id);
}
