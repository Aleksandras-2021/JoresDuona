using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class OrderServiceEntityConfiguration : IEntityTypeConfiguration<OrderService>
{
    public void Configure(EntityTypeBuilder<OrderService> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.Service)
            .WithMany(x => x.OrderServices)
            .HasForeignKey(x => x.ServiceId);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderServices)
            .HasForeignKey(x => x.OrderId);
    }
}