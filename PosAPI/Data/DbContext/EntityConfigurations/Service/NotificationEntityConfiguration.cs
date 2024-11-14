using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PosShared.Models;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class NotificationEntityConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Type)
            .HasConversion(new EnumToStringConverter<NotificationType>());
        builder.Property(x => x.SentAt).HasDefaultValue(DateTime.UtcNow);

        builder.HasOne(x => x.Reservation)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.ReservationId);
    }
}