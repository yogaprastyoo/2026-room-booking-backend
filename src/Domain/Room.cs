namespace RoomBooking.Api.Domain;

public class Room
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public Building Building { get; set; } = null!;
}
