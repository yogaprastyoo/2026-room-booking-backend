namespace RoomBooking.Api.DTOs.Booking;

public class UpdateBookingStatusRequest
{
    public string Status { get; set; } = string.Empty;  // "Pending", "Approved", "Rejected"
}
