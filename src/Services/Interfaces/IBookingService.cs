using RoomBooking.Api.DTOs.Booking;
using RoomBooking.Api.DTOs.Common;

namespace RoomBooking.Api.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest request);
    Task<BookingResponse?> GetByIdAsync(Guid id);
    Task<PagedResult<BookingResponse>> GetAllAsync(BookingFilterRequest filter);
    Task<BookingResponse?> UpdateAsync(Guid id, UpdateBookingRequest request);
    Task<BookingResponse?> UpdateStatusAsync(Guid id, UpdateBookingStatusRequest request);
    Task<bool> DeleteAsync(Guid id);
}
