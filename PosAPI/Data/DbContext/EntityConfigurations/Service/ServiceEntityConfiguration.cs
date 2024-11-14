using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class ServiceEntityConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.Business)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.EmployeeId);

        builder.HasMany(x => x.Reservations)
            .WithOne(x => x.Service)
            .HasForeignKey(x => x.ServiceId);

        builder.HasMany(x => x.OrderServices)
            .WithOne(x => x.Service)
            .HasForeignKey(x => x.ServiceId);
    }
}