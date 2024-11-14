using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class DiscountEntityConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasMany(x => x.OrderDiscounts)
            .WithOne(x => x.Discount)
            .HasForeignKey(x => x.DiscountId);

        builder.HasMany(x => x.OrderItemDiscounts)
            .WithOne(x => x.Discount)
            .HasForeignKey(x => x.DiscountId);

        builder.HasOne(x => x.Business)
            .WithMany(x => x.Discounts)
            .HasForeignKey(x => x.BusinessId);
    }
}