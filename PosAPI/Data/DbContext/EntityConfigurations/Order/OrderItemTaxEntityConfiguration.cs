using PosShared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class OrderItemTaxEntityConfiguration : IEntityTypeConfiguration<OrderItemTax>
{
    public void Configure(EntityTypeBuilder<OrderItemTax> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.OrderItem)
            .WithMany(x => x.OrderItemTaxes)
            .HasForeignKey(x => x.OrderItemId);
    }
}