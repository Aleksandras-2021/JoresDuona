using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class OrderItemDiscountEntityConfiguration : IEntityTypeConfiguration<OrderItemDiscount>
{
    public void Configure(EntityTypeBuilder<OrderItemDiscount> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.Discount)
            .WithMany(x => x.OrderItemDiscounts)
            .HasForeignKey(x => x.DiscountId);

        builder.HasOne(x => x.OrderItem)
            .WithMany(x => x.OrderItemDiscounts)
            .HasForeignKey(x => x.OrderItemId);
    }
}