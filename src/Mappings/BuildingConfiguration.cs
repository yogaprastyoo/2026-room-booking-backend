using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.Api.Domain;

namespace RoomBooking.Api.Mappings;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        // Table name
        builder.ToTable("buildings");

        // Primary key
        builder.HasKey(b => b.Id);

        // Id configuration - prevent database-generated values
        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        // Name configuration
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Code configuration
        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(20);

        // Unique constraints with explicit database names
        builder.HasIndex(b => b.Name)
            .IsUnique()
            .HasDatabaseName("ux_buildings_name");

        builder.HasIndex(b => b.Code)
            .IsUnique()
            .HasDatabaseName("ux_buildings_code");

        // Timestamp columns (timestamptz handled by ApplicationDbContext)
        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();
    }
}
