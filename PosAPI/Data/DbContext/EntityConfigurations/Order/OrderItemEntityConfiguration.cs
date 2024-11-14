using PosShared.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.Item)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.ItemId);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.OrderId);

        builder.HasMany(x => x.OrderItemTaxes)
            .WithOne(x => x.OrderItem)
            .HasForeignKey(x => x.OrderItemId);

        builder.HasMany(x => x.OrderItemDiscounts)
            .WithOne(x => x.OrderItem)
            .HasForeignKey(x => x.OrderItemId);

        builder.HasMany(x => x.OrderItemVariations)
            .WithOne(x => x.OrderItem)
            .HasForeignKey(x => x.OrderItemId);
    }
}