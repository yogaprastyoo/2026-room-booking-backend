using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.Api.Domain;

namespace RoomBooking.Api.Mappings;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        // Table name
        builder.ToTable("rooms", t => t.HasCheckConstraint(
            "chk_rooms_capacity_positive", 
            "capacity > 0"
        ));

        // Primary key
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        // Foreign key to Building
        builder.Property(r => r.BuildingId)
            .IsRequired();

        builder.HasOne(r => r.Building)
            .WithMany(b => b.Rooms)
            .HasForeignKey(r => r.BuildingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_rooms_building");

        // Name property
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Capacity property
        builder.Property(r => r.Capacity)
            .IsRequired();

        // Composite unique constraint: building_id + name
        builder.HasIndex(r => new { r.BuildingId, r.Name })
            .IsUnique()
            .HasDatabaseName("ux_rooms_building_id_name");

        // Timestamp properties
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();
    }
}
