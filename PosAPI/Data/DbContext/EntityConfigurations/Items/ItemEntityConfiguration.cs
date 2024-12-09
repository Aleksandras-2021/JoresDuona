using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class ItemEntityConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasMany(x => x.ItemVariations)
            .WithOne(x => x.Item)
            .HasForeignKey(x => x.ItemId);

        builder.HasOne(x => x.Business)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.BusinessId);

        builder.HasMany(x => x.OrderItems)
            .WithOne(x => x.Item)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict); // Disable cascade delete
    }
}