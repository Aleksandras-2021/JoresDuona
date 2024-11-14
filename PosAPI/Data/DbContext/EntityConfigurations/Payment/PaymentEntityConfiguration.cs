using PosShared.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PosAPI.Data.DbContext.EntityConfigurations;

public class PaymentEntityConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.PaymentDate).HasDefaultValue(DateTime.UtcNow);
        builder.Property(x => x.PaymentMethod)
            .HasConversion(new EnumToStringConverter<PaymentMethod>());
        builder.Property(x => x.PaymentGateway)
            .HasConversion(new EnumToStringConverter<PaymentGateway>());

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId);

        builder.HasMany(x => x.Refunds)
            .WithOne(x => x.Payment)
            .HasForeignKey(x => x.PaymentId);
    }
}