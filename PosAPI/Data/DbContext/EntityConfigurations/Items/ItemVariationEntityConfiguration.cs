using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class ItemVariationEntityConfiguration : IEntityTypeConfiguration<ItemVariation>
{
    public void Configure(EntityTypeBuilder<ItemVariation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.Item)
            .WithMany(x => x.ItemVariations)
            .HasForeignKey(x => x.ItemId);

        builder.HasMany(x => x.OrderItemVariations)
            .WithOne(x => x.ItemVariation)
            .HasForeignKey(x => x.ItemVariationId);
    }
}