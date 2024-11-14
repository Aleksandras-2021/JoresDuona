using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class OrderItemVariationEntityConfiguration : IEntityTypeConfiguration<OrderItemVariation>
{
    public void Configure(EntityTypeBuilder<OrderItemVariation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.ItemVariation)
            .WithMany(x => x.OrderItemVariations)
            .HasForeignKey(x => x.ItemVariationId);

        builder.HasOne(x => x.OrderItem)
            .WithMany(x => x.OrderItemVariations)
            .HasForeignKey(x => x.OrderItemId);
    }
}