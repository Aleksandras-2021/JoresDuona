using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class ScheduleEntityConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.LastUpdate).HasDefaultValue(DateTime.UtcNow);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.UserId);
    }
}