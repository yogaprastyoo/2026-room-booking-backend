namespace RoomBooking.Api.Domain;

public class Booking
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public DateTime BookingStart { get; set; }
    public DateTime BookingEnd { get; set; }
    public string? Notes { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public Room Room { get; set; } = null!;
}
