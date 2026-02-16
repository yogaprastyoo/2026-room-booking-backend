using System.ComponentModel.DataAnnotations;

namespace RoomBooking.Api.DTOs.Booking;

public class UpdateBookingRequest
{
    // RoomId NOT included - cannot be changed per PRD
    public string BorrowerName { get; set; } = string.Empty;
    [DataType(DataType.DateTime)]
    public DateTime BookingStart { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime BookingEnd { get; set; }
    public string? Notes { get; set; }
    // Status NOT included - separate endpoint per PRD
}
