using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.Api.Domain;

namespace RoomBooking.Api.Mappings;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // Table name with check constraints
        builder.ToTable("bookings", t =>
        {
            t.HasCheckConstraint(
                "chk_bookings_end_after_start",
                "booking_end > booking_start"
            );
            t.HasCheckConstraint(
                "chk_bookings_status",
                "status IN ('Pending', 'Approved', 'Rejected')"
            );
        });

        // Primary key
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        // Foreign key to Room
        builder.Property(b => b.RoomId)
            .IsRequired();

        builder.HasOne(b => b.Room)
            .WithMany()
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_bookings_room");

        // BorrowerName property
        builder.Property(b => b.BorrowerName)
            .IsRequired()
            .HasMaxLength(100);

        // BookingStart and BookingEnd
        builder.Property(b => b.BookingStart)
            .IsRequired();

        builder.Property(b => b.BookingEnd)
            .IsRequired();

        // Notes property (nullable)
        builder.Property(b => b.Notes)
            .HasMaxLength(500);

        // Status property (enum stored as string)
        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Timestamp properties
        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        // Soft delete property
        builder.Property(b => b.DeletedAt)
            .IsRequired(false);

        // Global query filter for soft delete
        builder.HasQueryFilter(b => b.DeletedAt == null);

        // Composite index for overlap detection performance
        builder.HasIndex(b => new { b.RoomId, b.BookingStart, b.BookingEnd })
            .HasDatabaseName("idx_booking_room_time");
    }
}
