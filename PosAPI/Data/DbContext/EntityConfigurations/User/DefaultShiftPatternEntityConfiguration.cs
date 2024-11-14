using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class DefaultShiftPatternEntityConfiguration : IEntityTypeConfiguration<DefaultShiftPattern>
{
    public void Configure(EntityTypeBuilder<DefaultShiftPattern> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.DayOfWeek).IsRequired()
            .HasConversion(new EnumToStringConverter<DayOfWeek>());

        builder.HasMany(x => x.Users)
            .WithMany(x => x.DefaultShiftPatterns)
            .UsingEntity(j => j.ToTable("UserDefaultShiftPatterns"));
    }
}