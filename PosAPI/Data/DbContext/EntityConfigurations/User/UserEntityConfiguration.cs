using PosShared.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Role)
            .HasConversion(new EnumToStringConverter<UserRole>());
        builder.Property(x => x.EmploymentStatus)
            .HasConversion(new EnumToStringConverter<EmploymentStatus>());

        builder.HasOne(x => x.Business)
            .WithMany(b => b.Users)
            .HasForeignKey(x => x.BusinessId);

        builder.HasMany(x => x.Schedules)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        builder.HasMany(x => x.TimeOffs)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        builder.HasMany(x => x.DefaultShiftPatterns)
            .WithMany(x => x.Users)
            .UsingEntity(j => j.ToTable("UserDefaultShiftPatterns"));

        builder.HasMany(x => x.Orders)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        builder.HasMany(x => x.Reservations)
            .WithOne(x => x.Employee)
            .HasForeignKey(x => x.EmployeeId);

        builder.HasMany(x => x.Services)
            .WithOne(x => x.Employee)
            .HasForeignKey(x => x.EmployeeId);
    }
}