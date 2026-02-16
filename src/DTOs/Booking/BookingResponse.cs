namespace RoomBooking.Api.DTOs.Booking;

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public DateTime BookingStart { get; set; }
    public DateTime BookingEnd { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;  // "Pending", "Approved", "Rejected"
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // DeletedAt NOT exposed per PRD
}
