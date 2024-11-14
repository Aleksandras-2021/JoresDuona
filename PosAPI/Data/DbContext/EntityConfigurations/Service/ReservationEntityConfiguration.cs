using PosShared.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class ReservationEntityConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CustomerPhone).HasMaxLength(20);
        builder.Property(x => x.Status)
            .HasConversion(new EnumToStringConverter<ReservationStatus>());
        builder.Property(x => x.BookedAt).HasDefaultValue(DateTime.UtcNow);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.ServiceId);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.EmployeeId);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.CustomerId);

        builder.HasMany(x => x.Notifications)
            .WithOne(x => x.Reservation)
            .HasForeignKey(x => x.ReservationId);

        builder.HasOne(x => x.Order)
            .WithOne(x => x.Reservation)
            .HasForeignKey<Reservation>(x => x.OrderId);
    }
}